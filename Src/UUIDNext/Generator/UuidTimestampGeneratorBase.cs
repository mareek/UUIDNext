using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Threading;

namespace UUIDNext.Generator
{
    public abstract class UuidTimestampGeneratorBase : UuidGeneratorBase
    {
        protected readonly RandomNumberGenerator _rng;
        private readonly int _sequenceMaxValue;

        private long _lastUsedTimestamp;
        private long _timestampOffset;
        private ushort _monotonicSequence;

        protected UuidTimestampGeneratorBase()
        {
            _rng = RandomNumberGenerator.Create();
            _sequenceMaxValue = (1 << SequenceBitSize) - 1;

            _lastUsedTimestamp = 0;
            _timestampOffset = 0;
            _monotonicSequence = 0;
        }

        protected abstract int SequenceBitSize { get; }

        public Guid New()
        {
            const int MaxAttempt = 10;
            int attemptCount = 0;
            do
            {
                if (TryGenerateNew(DateTime.UtcNow, out Guid newUuid))
                {
                    return newUuid;
                }
                attemptCount++;
                Thread.Sleep(1);
            } while (attemptCount < MaxAttempt);

            throw new Exception($"There are been too much attempt to generate an UUID withtin the last {MaxAttempt} ms");
        }

        protected abstract bool TryGenerateNew(DateTime date, out Guid newUuid);

        protected bool TrySetSequence(Span<byte> bytes, ref long timestamp)
        {
            var sequence = GetSequenceNumber(ref timestamp);
            if (sequence > _sequenceMaxValue)
            {
                return false;
            }

            BinaryPrimitives.TryWriteUInt16BigEndian(bytes, sequence);
            return true;
        }

        private ushort GetSequenceNumber(ref long timestamp)
        {
            lock (this)
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
            _rng.GetBytes(buffer);
            //Setting the highest bit to 0 mitigate the risk of a sequence overflow (see section 6.2)
            buffer[0] &= 0b0000_0111;
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }
    }
}
