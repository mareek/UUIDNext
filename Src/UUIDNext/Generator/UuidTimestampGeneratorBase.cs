using System.Security.Cryptography;

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

        protected int GetSequenceNumber(long timestamp)
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
