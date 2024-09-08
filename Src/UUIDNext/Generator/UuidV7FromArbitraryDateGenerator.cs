using System.Buffers.Binary;
using UUIDNext.Tools;

namespace UUIDNext.Generator;

/// <summary>
/// Generate a UUID version 7 given an arbitrary date
/// </summary>
/// <remarks>
/// To give the best possible UUID given an arbitrary date we can't rely on the same mechanism as in UuidV7Generator
/// </remarks>
internal class UuidV7FromArbitraryDateGenerator
{
    private QDCache<long, MonotonicityHandler> _monotonicityHandlerByTimestamp = new(100);

    private Guid New(DateTimeOffset date)
    {
        var dateTimeStamp = date.ToUnixTimeMilliseconds();
        var monotonicityHandler = _monotonicityHandlerByTimestamp.GetOrAdd(dateTimeStamp, CreateV7Handler);
        var (computedTimestamp, sequence) = monotonicityHandler.GetTimestampAndSequence(date);

        if (computedTimestamp > dateTimeStamp)
        {
            _monotonicityHandlerByTimestamp.Set(dateTimeStamp, CreateV7Handler(default));
            (computedTimestamp, sequence) = monotonicityHandler.GetTimestampAndSequence(date);
        }

        Span<byte> sequenceBytes = stackalloc byte[2];
        BinaryPrimitives.TryWriteUInt16BigEndian(sequenceBytes, sequence);

        return UuidToolkit.CreateUuidV7(computedTimestamp, sequenceBytes);

        static MonotonicityHandler CreateV7Handler(long _) => new(sequenceBitSize: 12);
    }
}
