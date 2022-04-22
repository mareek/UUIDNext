using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    public abstract class UuidTimestampGeneratorBase : UuidGeneratorBase
    {
        private readonly int _sequenceMaxValue;

        private long _lastUsedTimestamp;
        private long _timestampOffset;
        private ushort _monotonicSequence;

        protected UuidTimestampGeneratorBase()
        {
            _sequenceMaxValue = (1 << SequenceBitSize) - 1;

            _lastUsedTimestamp = 0;
            _timestampOffset = 0;
            _monotonicSequence = 0;
        }

        protected abstract int SequenceBitSize { get; }

        protected abstract Guid New(DateTime date);

        public Guid New() => New(DateTime.UtcNow);

        protected void SetSequence(Span<byte> bytes, ref long timestamp)
        {
            lock (this)
            {
                var sequence = GetSequenceNumber(ref timestamp);
                if (sequence > _sequenceMaxValue)
                {
                    // if the sequence is greater than the max value, we take advantage
                    // of the anti-rewind mechanis to simulate a slight change in clock time
                    _timestampOffset += 1;
                    sequence = GetSequenceNumber(ref timestamp);
                }

                BinaryPrimitives.TryWriteUInt16BigEndian(bytes, sequence);
            }
        }

        private ushort GetSequenceNumber(ref long timestamp)
        {
            EnsureTimestampNeverMoveBackward(ref timestamp);

            if (timestamp == _lastUsedTimestamp)
            {
                _monotonicSequence += 1;
            }
            else
            {
                _lastUsedTimestamp = timestamp;
                _monotonicSequence = GetSequenceSeed();
            }

            return _monotonicSequence;
        }

        private void EnsureTimestampNeverMoveBackward(ref long timestamp)
        {
            timestamp += _timestampOffset;
            // if the computer clock has moved backward since the last generated UUID,
            // we add an offset to ensure the timestamp always move forward (See RFC Section 6.2)
            if (timestamp < _lastUsedTimestamp)
            {
                _timestampOffset += _lastUsedTimestamp - timestamp;
                timestamp = _lastUsedTimestamp;
            }
        }

        private ushort GetSequenceSeed()
        {
            // following section 6.2 on "Fixed-Length Dedicated Counter Seeding", the initial value of the sequence is randomized
            Span<byte> buffer = stackalloc byte[2];
            RandomNumberGenerator.Fill(buffer);
            //Setting the highest bit to 0 mitigate the risk of a sequence overflow (see section 6.2)
            buffer[0] &= 0b0000_0111;
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }
    }
}
