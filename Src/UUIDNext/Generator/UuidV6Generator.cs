using System;
using System.Buffers.Binary;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 6 based on RFC draft at https://github.com/uuid6/uuid6-ietf-draft/
    /// </summary>
    public class UuidV6Generator : UuidTimestampGeneratorBase
    {
        private static readonly long GregorianCalendarStart = new DateTime(1582, 10, 15, 0, 0, 0, DateTimeKind.Utc).Ticks;

        protected override byte Version => 6;

        public Guid New() => NewInternal(DateTime.UtcNow);

        //For unit tests
        internal Guid NewInternal(DateTime date)
        {
            /* UUID V6 layout
              0                   1                   2                   3
              0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                           time_high                           |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |           time_mid            |      time_low_and_version     |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |clk_seq_hi_res |  clk_seq_low  |         node (0-1)            |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                         node (2-5)                            |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             */

            Span<byte> bytes = stackalloc byte[16];
            long timestamp = date.Ticks - GregorianCalendarStart;
            SetTimestampBytes(bytes[0..8], timestamp);
            SetSequenceBytes(bytes[8..10], timestamp);
            _rng.GetBytes(bytes[10..16]);
            return CreateGuidFromBigEndianBytes(bytes);
        }

        private static void SetTimestampBytes(Span<byte> bytes, long timestamp)
        {
            BinaryPrimitives.TryWriteInt64BigEndian(bytes, timestamp << 4);
            //shift lower 12 bits to make room for version Bits
            bytes[7] = (byte)((bytes[6] << 4) | (bytes[7] >> 4));
            bytes[6] = (byte)(bytes[6] >> 4);
        }

        private void SetSequenceBytes(Span<byte> bytes, long unixTimeStamp)
        {
            short sequence = GetSequenceNumber(unixTimeStamp);
            BinaryPrimitives.TryWriteInt16BigEndian(bytes, sequence);
        }
    }
}
