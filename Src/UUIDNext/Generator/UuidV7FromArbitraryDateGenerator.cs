using System.Buffers.Binary;
using UUIDNext.Tools;

namespace UUIDNext.Generator;

/// <summary>
/// Generate a UUID version 7 given an arbitrary date
/// </summary>
/// <remarks>
/// To give the best possible UUID given an arbitrary date we can't rely on UuidV7Generator because it has some 
/// mechanism to ensure that every UUID generated is greater then the previous one.
/// This generator try to find the best compromise between these three pillars:
/// * The timestamp part should always represent the date parameter. period.
/// * We should stay as close as possible to the "spirit" of UUID V7 and provide incresing value for a given date
/// * This library should be as lightweight as possible
/// The first point implies that there shouldn't be overflow preventing mechanism like in UuidV7Generator. The second
/// point implies that we should keep track of the monotonicity of multiple timestamps in parallel. The third point 
/// implies that the number of timestamps we keep track of should be limited.
/// </remarks>
internal class UuidV7FromArbitraryDateGenerator
{
    // From the few bench I've done this cache should consume 15KB of memory at most with a capacity of 256
    private QDCache<long, MonotonicityHandler> _monotonicityHandlerByTimestamp = new(256);

    /// <summary>
    /// Create a UUID version 7 where the timestamp part represent the given date
    /// </summary>
    /// <param name="date">The date that will provide the timestamp par of the UUID</param>
    /// <returns>A UUID version 7</returns>
    public Guid New(DateTimeOffset date)
    {
        var dateTimeStamp = date.ToUnixTimeMilliseconds();
        var monotonicityHandler = _monotonicityHandlerByTimestamp.GetOrAdd(dateTimeStamp, CreateV7Handler);
        var (computedTimestamp, sequence) = monotonicityHandler.GetTimestampAndSequence(date);

        // if the timestamp given by the monotonicityHandler is greater than the the date parameter that means that 
        // the sequence have overflowed so we reset the sequence by creating a new monotonicityHandler
        if (computedTimestamp > dateTimeStamp)
        {
            monotonicityHandler = CreateV7Handler(default);
            _monotonicityHandlerByTimestamp.Set(dateTimeStamp, monotonicityHandler);
            (computedTimestamp, sequence) = monotonicityHandler.GetTimestampAndSequence(date);
        }

        Span<byte> sequenceBytes = stackalloc byte[2];
        BinaryPrimitives.TryWriteUInt16BigEndian(sequenceBytes, sequence);

        return UuidToolkit.CreateUuidV7(computedTimestamp, sequenceBytes);

        static MonotonicityHandler CreateV7Handler(long _) => new(sequenceBitSize: 12);
    }
}
