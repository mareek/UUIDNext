using System;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator;

public class UuidV8SqlServerFromSpecificDateGeneratorTest : UuidFromSpecificDateGeneratorTestBase
{
    protected override byte Version => 8;

    protected override int SequenceBitSize => 14;

    protected override object NewGenerator() => new UuidV8SqlServerFromSpecificDateGenerator();

    protected override Guid NewUuid(object generator)
        => ((UuidV8SqlServerFromSpecificDateGenerator)generator).New(DateTimeOffset.Now);

    protected override Guid NewUuid(object generator, DateTimeOffset date)
        => ((UuidV8SqlServerFromSpecificDateGenerator)generator).New(date);
}
