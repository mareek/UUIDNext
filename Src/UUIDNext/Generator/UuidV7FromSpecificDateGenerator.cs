using UUIDNext.Tools;

namespace UUIDNext.Generator;

/// <summary>
/// Generate a UUID version 7 given an arbitrary date
/// </summary>
internal class UuidV7FromSpecificDateGenerator(int cacheSize = 1024)
    : UuidFromSpecificDateGeneratorBase(sequenceMaxValue: 0b1111_1111_1111, cacheSize: cacheSize)
{
    protected override Guid CreateUuid(long timestamp, Span<byte> sequenceBytes)
        => UuidToolkit.CreateUuidV7(timestamp, sequenceBytes);
}
