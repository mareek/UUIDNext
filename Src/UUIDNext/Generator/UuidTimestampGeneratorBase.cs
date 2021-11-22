using System;
using System.Security.Cryptography;
using System.Threading;

namespace UUIDNext.Generator
{
    public abstract class UuidTimestampGeneratorBase : UuidGeneratorBase
    {
        protected readonly RandomNumberGenerator _rng;

        private long _lastUsedTimestamp;
        private int _monotonicSequence;

        protected UuidTimestampGeneratorBase()
        {
            _rng = RandomNumberGenerator.Create();
            _lastUsedTimestamp = 0;
            _monotonicSequence = 0;
        }

        protected abstract int SequenceMaxValue { get; }

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

        protected bool TryGetSequenceNumber(long timestamp, out int sequence)
        {
            sequence = GetSequenceNumber(timestamp);
            return sequence <= SequenceMaxValue;
        }

        private int GetSequenceNumber(long timestamp)
        {
            lock (this)
            {
                if (timestamp == _lastUsedTimestamp)
                {
                    _monotonicSequence += 1;
                }
                else
                {
                    _lastUsedTimestamp = timestamp;
                    _monotonicSequence = 0;
                }

                return _monotonicSequence;
            }
        }
    }
}
