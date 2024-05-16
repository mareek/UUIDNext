using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using UUIDNext.Tools;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 8 based on RFC draft at https://github.com/ietf-wg-uuidrev/rfc4122bis
    /// </summary>
    /// <remarks>
    /// In SQL Server UUIDs stored in a column of type uniqueidentifier are not sorted in the order of the bytes (see #2).
    /// This class generate UUID similar to UUID v7 but with a different byte order so that the UUIDs are sorted
    /// when used in a uniqueidentifier typed column in SQL Sever
    /// </remarks>
    public class UuidV8SqlServerGenerator : UuidTimestampGeneratorBase
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

            long timestampInMs = ((DateTimeOffset)date).ToUnixTimeMilliseconds();

            SetSequence(bytes.Slice(8,2), ref timestampInMs);
            SetTimestamp(bytes.Slice(10, 6), timestampInMs);
            bytes.Slice(0, 8).FillWithRandom();

            return CreateGuidFromBigEndianBytes(bytes);
        }

        private void SetTimestamp(Span<byte> bytes, long timestampInMs)
        {
            Span<byte> timestampInMillisecondsBytes = stackalloc byte[8];
            BinaryPrimitives.TryWriteInt64BigEndian(timestampInMillisecondsBytes, timestampInMs);
            timestampInMillisecondsBytes.Slice(2, 6).CopyTo(bytes);
        }

        [Obsolete("Use UuidDecoder.DecodeUuidV8ForSqlServer instead. This function will be removed in the next version")]
        public static (long timestampMs, short sequence) Decode(Guid guid) => UuidDecoder.DecodeUuidV8ForSqlServer(guid);
    }
}
