using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 7 based on RFC draft at https://github.com/uuid6/uuid6-ietf-draft/
    /// </summary>
    public class UuidV7Generator : UuidGeneratorBase
    {
        private readonly RandomNumberGenerator _rng;

        private long _lastUsedTimestampInMs;
        private short _monotonicSequence;

        public UuidV7Generator()
        {
            _rng = RandomNumberGenerator.Create();
            _lastUsedTimestampInMs = 0;
            _monotonicSequence = 0;
        }

        protected override byte Version => 7;

        public Guid New()
        {
            /* We implement the first example given in section 4.4.4.1 of the RFC
              0                   1                   2                   3
              0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                            unixts                             |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |unixts |         msec          |  ver  |          seq          |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |var|                         rand                              |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                             rand                              |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ 
             */

            Span<byte> bytes = stackalloc byte[16];

            var unixTimeStamp = DateTime.UtcNow - DateTime.UnixEpoch;

            SetTimestamp(bytes[0..5], unixTimeStamp);
            SetSubSecA(bytes[4..6], unixTimeStamp);
            SetSubSecB(bytes[6..8], unixTimeStamp);
            _rng.GetBytes(bytes[8..16]);

            return CreateGuidFromBigEndianBytes(bytes);
        }

        private void SetTimestamp(Span<byte> bytes, TimeSpan unixTimeStamp)
        {
            long timestampInSeconds = Convert.ToInt64(Math.Floor(unixTimeStamp.TotalSeconds));
            // The timestamp is stored on 36 bits (4 and a half bytes) so we shift the
            // timestamp value by 4 bits and copy the 5 less significant bytes of the timestamp
            long shiftedTimestamp = timestampInSeconds << 4;
            Span<byte> timestampinSecondsBytes = stackalloc byte[8];
            BitConverter.TryWriteBytes(timestampinSecondsBytes, shiftedTimestamp);
            if (BitConverter.IsLittleEndian)
            {
                bytes[0] = timestampinSecondsBytes[4];
                bytes[1] = timestampinSecondsBytes[3];
                bytes[2] = timestampinSecondsBytes[2];
                bytes[3] = timestampinSecondsBytes[1];
                bytes[4] = timestampinSecondsBytes[0];
            }
            else
            {
                timestampinSecondsBytes[3..8].CopyTo(bytes);
            }
        }

        private void SetSubSecA(Span<byte> bytes, TimeSpan unixTimeStamp)
        {
            var timestampMs = (short)unixTimeStamp.Milliseconds;
            Span<byte> timestampMsBytes = stackalloc byte[2];
            BitConverter.TryWriteBytes(timestampMsBytes, timestampMs);
            if (BitConverter.IsLittleEndian)
            {
                // this byte is shared with the last 4 bits of the timestamp.
                // as the 6 upper bits of the milliseconds will alaways be 0 we can simply add the two bytes
                bytes[0] |= timestampMsBytes[1];
                bytes[1] = timestampMsBytes[0];
            }
            else
            {
                bytes[0] |= timestampMsBytes[0];
                bytes[1] = timestampMsBytes[1];
            }
        }

        private void SetSubSecB(Span<byte> bytes, TimeSpan unixTimeStamp)
        {
            var timestampInMs = Convert.ToInt64(Math.Floor(unixTimeStamp.TotalMilliseconds));
            short sequence = GetSequenceNumber(timestampInMs);
            BinaryPrimitives.TryWriteInt16BigEndian(bytes, sequence);
        }

        private short GetSequenceNumber(long timestampInMs)
        {
            lock (this)
            {
                if (timestampInMs == _lastUsedTimestampInMs)
                {
                    _monotonicSequence += 1;
                }
                else
                {
                    _lastUsedTimestampInMs = timestampInMs;
                    _monotonicSequence = 0;
                }

                return _monotonicSequence;
            }
        }
    }
}
