using System;
using UUIDNext.Generator;

namespace UUIDNext.Test.Generator
{
    public class UuidV7GeneratorTest : UuidTimestampGeneratorBaseTest
    {
        private readonly UuidV7Generator _generator = new();

        protected override byte Version => 7;

        protected override Guid NewUuid() => _generator.New();
    }
}
