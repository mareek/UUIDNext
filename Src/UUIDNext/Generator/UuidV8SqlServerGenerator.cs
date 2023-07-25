using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 8 based on RFC draft at https://github.com/ietf-wg-uuidrev/rfc4122bis
    /// </summary>
    internal class UuidV8SqlServerGenerator : UuidTimestampGeneratorBase
    {
        protected override byte Version => 8;

        protected override int SequenceBitSize => 14;

        protected override Guid New(DateTime date)
        {
            /* This structure should produce ordered UUIDs in SQL Server
              0                   1                   2                   3
              0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                              rand                             |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |             rand              |  ver  |         rand          |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |var|         sequence          |          unix_ts_ms           |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                           unix_ts_ms                          |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             */

            Span<byte> bytes = stackalloc byte[16];

            TimeSpan unixTimeStamp = date - DateTime.UnixEpoch;
            long timestampInMs = Convert.ToInt64(Math.Floor(unixTimeStamp.TotalMilliseconds));

            SetSequence(bytes[8..10], ref timestampInMs);
            SetTimestamp(bytes[10..16], timestampInMs);
            RandomNumberGenerator.Fill(bytes[0..8]);

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
            bytes[10..16].CopyTo(timestampBytes[2..8]);
            long timestampMs = BinaryPrimitives.ReadInt64BigEndian(timestampBytes);

            var sequenceBytes = bytes[8..10];
            //remove variant information
            sequenceBytes[0] &= 0b0011_1111;
            short sequence = BinaryPrimitives.ReadInt16BigEndian(sequenceBytes);

            return (timestampMs, sequence);
        }
    }
}
