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
/// After some benchmarks, I chose a cache size of 256 entries. The cache has a memory footprint of only a few KB and 
/// has a reasonable worst case performance
/// </remarks>
internal class UuidV7FromArbitraryDateGenerator(int cacheSize = 256)
{
    private const ushort SequenceMaxValue = 0b1111_1111_1111;

    private QDCache<long, ushort> _sequenceByTimestamp = new(cacheSize);
    
    /// <summary>
    /// Create a UUID version 7 where the timestamp part represent the given date
    /// </summary>
    /// <param name="date">The date that will provide the timestamp par of the UUID</param>
    /// <returns>A UUID version 7</returns>
    public Guid New(DateTimeOffset date)
    {
        var timestamp = date.ToUnixTimeMilliseconds();
        ushort sequence = ComputeSequence(timestamp);

        Span<byte> sequenceBytes = stackalloc byte[2];
        BinaryPrimitives.TryWriteUInt16BigEndian(sequenceBytes, sequence);

        return UuidToolkit.CreateUuidV7(timestamp, sequenceBytes);
    }

    private ushort ComputeSequence(long timestamp)
    {
        var sequence = _sequenceByTimestamp.GetOrAdd(timestamp, GetSequenceSeed);
        if (sequence < SequenceMaxValue)
            sequence += 1;
        else
            sequence = GetSequenceSeed(default);

        _sequenceByTimestamp.Set(timestamp, sequence);
        return sequence;

        static ushort GetSequenceSeed(long _)
        {
            // following section 6.2 on "Fixed-Length Dedicated Counter Seeding", the initial value of the sequence is randomized
            Span<byte> buffer = stackalloc byte[2];
            RandomNumberGeneratorPolyfill.Fill(buffer);
            // Setting the highest bit to 0 mitigate the risk of a sequence overflow (see section 6.2)
            buffer[0] &= 0b0000_0111;
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }
    }
}
