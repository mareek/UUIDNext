using System.Buffers.Binary;

namespace UUIDNext.Generator;

/// <summary>
/// Generate a UUID with a timestamp given an arbitrary date
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
/// After some benchmarks, I chose a cache size of 1024 entries. The cache has a memory footprint of only a few 
/// dozen KB and has a reasonable worst case performance
/// </remarks>
internal abstract class UuidFromSpecificDateGeneratorBase(ushort sequenceMaxValue, int cacheSize)
{

    private readonly SequenceManager _sequenceManager = new(sequenceMaxValue, cacheSize);

    protected abstract Guid CreateUuid(long timestamp, Span<byte> sequenceBytes);

    /// <summary>
    /// Create a UUID version 7 where the timestamp part represent the given date
    /// </summary>
    /// <param name="date">The date that will provide the timestamp part of the UUID</param>
    /// <returns>A UUID version 7</returns>
    public Guid New(DateTimeOffset date)
    {
        long timestamp = date.ToUnixTimeMilliseconds();
        ushort sequence = _sequenceManager.ComputeSequence(timestamp);

        Span<byte> sequenceBytes = stackalloc byte[2];
        BinaryPrimitives.TryWriteUInt16BigEndian(sequenceBytes, sequence);

        return CreateUuid(timestamp, sequenceBytes);
    }
}