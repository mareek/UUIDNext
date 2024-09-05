using System.Buffers.Binary;
using UUIDNext.Tools;

namespace UUIDNext.Generator;

/// <summary>
/// Generate a UUID version 7 based on RFC 9562
/// </summary>
public class UuidV7Generator
{
    private readonly MonotonicityHandler _monotonicityHandler = new(sequenceBitSize: 12);

    internal Guid New() => New(DateTimeOffset.UtcNow);

    private Guid New(DateTimeOffset date)
    {
        /* We implement the following bit layout as per section 5.7 of the RFC
          0                   1                   2                   3
          0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |                            timestamp                          |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |           timestamp           |  ver  |       sequence        |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |var|                        random                             |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |                            random                             |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         */

        var (timestamp, sequence) = _monotonicityHandler.GetTimestampAndSequence(date);

        Span<byte> sequenceBytes = stackalloc byte[2];
        BinaryPrimitives.TryWriteUInt16BigEndian(sequenceBytes, sequence);

        return UuidToolkit.CreateUuidV7(timestamp, sequenceBytes);
    }

    [Obsolete("Use UuidDecoder.DecodeUuidV7 instead. This function will be removed in the next version")]
    public static (long timestampMs, short sequence) Decode(Guid guid) => UuidDecoder.DecodeUuidV7(guid);
}
