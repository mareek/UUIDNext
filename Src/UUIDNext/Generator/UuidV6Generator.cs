using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 6 based on RFC draft at https://github.com/uuid6/uuid6-ietf-draft/
    /// </summary>
    public class UuidV6Generator : UuidTimestampGeneratorBase
    {
        private static readonly long GregorianCalendarStart = new DateTime(1582, 10, 15, 0, 0, 0, DateTimeKind.Utc).Ticks;

        protected override byte Version => 6;

        protected override int SequenceBitSize => 14;

        protected override Guid New(DateTime date)
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

            SetSequence(bytes[8..10], ref timestamp);
            SetTimestamp(bytes[0..8], timestamp);
            RandomNumberGenerator.Fill(bytes[10..16]);

            return CreateGuidFromBigEndianBytes(bytes);
        }

        private static void SetTimestamp(Span<byte> bytes, long timestamp)
        {
            BinaryPrimitives.TryWriteInt64BigEndian(bytes, timestamp << 4);
            //shift lower 12 bits to make room for version Bits
            bytes[7] = (byte)((bytes[6] << 4) | (bytes[7] >> 4));
            bytes[6] = (byte)(bytes[6] >> 4);
        }

        public static (long timestamp, short sequence) Decode(Guid guid)
        {
            Span<byte> bytes = stackalloc byte[16];
            GuidHelper.TryWriteBigEndianBytes(guid, bytes);

            var timestampBytes = bytes[0..8];
            //remove version information
            timestampBytes[6] = (byte)((timestampBytes[6] << 4) | (timestampBytes[7] >> 4));
            timestampBytes[7] = (byte)(timestampBytes[7] << 4);
            long timestamp = BinaryPrimitives.ReadInt64BigEndian(timestampBytes) >> 4;

            var sequenceBytes = bytes[8..10];
            //remove variant information
            sequenceBytes[0] &= 0b0011_1111;
            short sequence = BinaryPrimitives.ReadInt16BigEndian(sequenceBytes);

            return (timestamp, sequence);
        }
    }
}
