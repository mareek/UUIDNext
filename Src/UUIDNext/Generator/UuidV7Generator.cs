using System;
using System.Buffers.Binary;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 7 based on RFC draft at https://github.com/uuid6/uuid6-ietf-draft/
    /// </summary>
    public class UuidV7Generator : UUIDSequenceGeneratorBase
    {
        protected override byte Version => 7;

        public Guid New() => NewInternal(DateTime.UtcNow);

        //For unit tests
        internal Guid NewInternal(DateTime date)
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

            TimeSpan unixTimeStamp = date - DateTime.UnixEpoch;

            SetTimestampSeconds(bytes[0..5], unixTimeStamp);
            SetTimestampMs(bytes[4..6], unixTimeStamp);
            SetSequence(bytes[6..8], unixTimeStamp);
            _rng.GetBytes(bytes[8..16]);

            return CreateGuidFromBigEndianBytes(bytes);
        }

        private void SetTimestampSeconds(Span<byte> bytes, TimeSpan unixTimeStamp)
        {
            long timestampInSeconds = Convert.ToInt64(Math.Floor(unixTimeStamp.TotalSeconds));
            // The timestamp is stored on 36 bits (4 and a half bytes) so we shift the
            // timestamp value by 4 bits and copy the 5 less significant bytes of the timestamp
            long shiftedTimestamp = timestampInSeconds << 4;
            Span<byte> timestampinSecondsBytes = stackalloc byte[8];
            BinaryPrimitives.TryWriteInt64BigEndian(timestampinSecondsBytes, shiftedTimestamp);
            timestampinSecondsBytes[3..8].CopyTo(bytes);
        }

        private void SetTimestampMs(Span<byte> bytes, TimeSpan unixTimeStamp)
        {
            short timestampMs = (short)unixTimeStamp.Milliseconds;
            Span<byte> timestampMsBytes = stackalloc byte[2];
            BinaryPrimitives.TryWriteInt16BigEndian(timestampMsBytes, timestampMs);
            // this byte is shared with the last 4 bits of the timestamp.
            // as the 6 upper bits of the milliseconds will alaways be 0 we can simply add the two bytes
            bytes[0] |= timestampMsBytes[0];
            bytes[1] = timestampMsBytes[1];
        }

        private void SetSequence(Span<byte> bytes, TimeSpan unixTimeStamp)
        {
            long timestampInMs = Convert.ToInt64(Math.Floor(unixTimeStamp.TotalMilliseconds));
            short sequence = GetSequenceNumber(timestampInMs);
            BinaryPrimitives.TryWriteInt16BigEndian(bytes, sequence);
        }
    }
}
