using System;
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

        protected bool TryGetSequenceNumber(ref long timestamp, out ushort sequence)
        {
            sequence = GetSequenceNumber(ref timestamp);
            return sequence <= _sequenceMaxValue;
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

        protected abstract ushort GetSequenceSeed();
    }
}
