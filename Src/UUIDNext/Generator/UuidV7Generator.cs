using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 7 based on RFC draft at https://github.com/ietf-wg-uuidrev/rfc4122bis
    /// </summary>
    internal class UuidV7Generator : UuidTimestampGeneratorBase
    {
        protected override byte Version => 7;

        protected override int SequenceBitSize => 12;

        protected override Guid New(DateTime date)
        {
            /* We implement the first example given in section 4.4.4.1 of the RFC
              0                   1                   2                   3
              0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                           unix_ts_ms                          |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |          unix_ts_ms           |  ver  |       rand_a          |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |var|                        rand_b                             |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                            rand_b                             |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             */

            Span<byte> bytes = stackalloc byte[16];

            TimeSpan unixTimeStamp = date - DateTime.UnixEpoch;
            long timestampInMs = Convert.ToInt64(Math.Floor(unixTimeStamp.TotalMilliseconds));

            SetSequence(bytes[6..8], ref timestampInMs);
            SetTimestamp(bytes[0..6], timestampInMs);
            RandomNumberGenerator.Fill(bytes[8..16]);

            return CreateGuidFromBigEndianBytes(bytes);
        }

        private void SetTimestamp(Span<byte> bytes, long timestampInMs)
        {
            Span<byte> timestampInMillisecondsBytes = stackalloc byte[8];
            BinaryPrimitives.TryWriteInt64BigEndian(timestampInMillisecondsBytes, timestampInMs);
            timestampInMillisecondsBytes[2..8].CopyTo(bytes);
        }

        public static (long timestampMs, short sequence) Decode(Guid guid)
        {
            Span<byte> bytes = stackalloc byte[16];
            guid.TryWriteBytes(bytes, bigEndian: true, out var _);

            Span<byte> timestampBytes = stackalloc byte[8];
            bytes[0..6].CopyTo(timestampBytes[2..8]);
            long timestampMs = BinaryPrimitives.ReadInt64BigEndian(timestampBytes);

            var sequenceBytes = bytes[6..8];
            //remove version information
            sequenceBytes[0] &= 0b0000_1111;
            short sequence = BinaryPrimitives.ReadInt16BigEndian(sequenceBytes);

            return (timestampMs, sequence);
        }
    }
}
