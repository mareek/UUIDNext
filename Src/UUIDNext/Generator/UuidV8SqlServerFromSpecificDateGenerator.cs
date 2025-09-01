using UUIDNext.Tools;

namespace UUIDNext.Generator;

/// <summary>
/// Generate a UUID version 8 optimised for SQL Server given an arbitrary date
/// </summary>
internal class UuidV8SqlServerFromSpecificDateGenerator(int cacheSize = 1024)
    : UuidFromSpecificDateGeneratorBase(sequenceMaxValue: 0b11_1111_1111_1111, cacheSize: cacheSize)

{
    protected override Guid CreateUuid(long timestamp, Span<byte> sequenceBytes)
        => UuidToolkit.CreateSequentialUuidForSqlServer(timestamp, sequenceBytes);
}
