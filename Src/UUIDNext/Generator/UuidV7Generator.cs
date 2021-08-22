using System;
using System.Buffers.Binary;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 7 based on RFC draft at https://github.com/uuid6/uuid6-ietf-draft/
    /// </summary>
    public class UuidV7Generator : UuidTimestampGeneratorBase
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

        public (long timestamp, short timestampMs, short sequence) Decode(Guid guid)
        {
            Span<byte> bytes = stackalloc byte[16];
            GuidHelper.TryWriteBigEndianBytes(guid, bytes);

            Span<byte> timestampBytes = stackalloc byte[8];
            bytes[0..5].CopyTo(timestampBytes[3..8]);
            long timestamp = BinaryPrimitives.ReadInt64BigEndian(timestampBytes) >> 4;

            var timestampMsBytes = bytes[4..6];
            //remove lower 4 bits of unix timestamp
            timestampMsBytes[0] &= 0b0000_1111;
            short timestampMs = BinaryPrimitives.ReadInt16BigEndian(timestampMsBytes);

            var sequenceBytes = bytes[6..8];
            //remove version information
            sequenceBytes[0] &= 0b0000_1111;
            short sequence = BinaryPrimitives.ReadInt16BigEndian(sequenceBytes);

            return (timestamp, timestampMs, sequence);
        }
    }
}
