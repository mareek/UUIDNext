using System.Buffers.Binary;
using UUIDNext.Tools;

namespace UUIDNext.Generator;

internal class SequenceManager(ushort sequenceMaxValue, int cacheSize)
{
    private readonly ushort SequenceMaxValue = sequenceMaxValue;
    // Setting the highest bit to 0 mitigate the risk of a sequence overflow (see section 6.2)
    private readonly byte HighestByteMask = (byte)(sequenceMaxValue >> 9);

    private readonly BetterCache<long, ushort> _sequenceByTimestamp = new(cacheSize);

    public ushort ComputeSequence(long timestamp)
    {
        if (timestamp < 0)
            throw new ArgumentOutOfRangeException(nameof(timestamp), "Dates before 1970-01-01 are not supported");

        return _sequenceByTimestamp.AddOrUpdate(timestamp,
                                                _ => GetSequenceSeed(),
                                                (_, s) => UpdateSequence(s));
    }

    private ushort UpdateSequence(ushort sequence)
           => sequence < SequenceMaxValue ? (ushort)(sequence + 1)
                                          : GetSequenceSeed();

    private ushort GetSequenceSeed()
    {
        // following section 6.2 on "Fixed-Length Dedicated Counter Seeding", the initial value of the sequence is randomized
        Span<byte> buffer = stackalloc byte[2];
        RandomNumberGeneratorPolyfill.Fill(buffer);
        // Setting the highest bit to 0 mitigate the risk of a sequence overflow (see section 6.2)
        buffer[0] &= HighestByteMask;
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }
}
