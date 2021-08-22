using System;
using UUIDNext.Generator;

namespace UUIDNext.Test.Generator
{
    public class UuidV6GeneratorTest : UuidTimestampGeneratorBaseTest
    {
        private readonly UuidV6Generator _generator = new();

        protected override byte Version => 6;

        protected override Guid NewUuid() => _generator.New();
    }
}
