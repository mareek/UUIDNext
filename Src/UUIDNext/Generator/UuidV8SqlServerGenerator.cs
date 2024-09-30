using System.Buffers.Binary;
using UUIDNext.Tools;

namespace UUIDNext.Generator;

/// <summary>
/// Generate a UUID version 8 based on RFC 9562
/// </summary>
/// <remarks>
/// In SQL Server UUIDs stored in a column of type uniqueidentifier are not sorted in the order of the bytes (see #2).
/// This class generate UUID similar to UUID v7 but with a different byte order so that the UUIDs are sorted
/// when used in a uniqueidentifier typed column in SQL Sever
/// </remarks>
internal class UuidV8SqlServerGenerator
{
    private readonly MonotonicityHandler _monotonicityHandler = new(sequenceBitSize: 14);

    public Guid New() => New(DateTimeOffset.UtcNow);

    private Guid New(DateTimeOffset date)
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

        var (timestamp, sequence) = _monotonicityHandler.GetTimestampAndSequence(date);

        Span<byte> bytes = stackalloc byte[16];
        // We only use 48 bits of the timestamp so we can write it on 64 bits and then
        // erase the 16 most significant bits with the sequence to save some buffer allocation and copy
        BinaryPrimitives.TryWriteInt64BigEndian(bytes.Slice(8, 8), timestamp);
        BinaryPrimitives.TryWriteUInt16BigEndian(bytes.Slice(8, 2), sequence);

        RandomNumberGeneratorPolyfill.Fill(bytes.Slice(0, 8));

        return UuidToolkit.CreateGuidFromBigEndianBytes(bytes, 8);
    }
}
