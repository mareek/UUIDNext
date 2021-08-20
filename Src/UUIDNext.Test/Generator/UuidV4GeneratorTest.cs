using System;
using System.Collections.Generic;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV4GeneratorTest
    {
        [Fact]
        public void DumbTest()
        {
            UuidV4Generator generator = new();
            HashSet<Guid> generatedUuids = new();
            for (int i = 0; i < 100; i++)
            {
                var newUuid = generator.New();
                UuidTestHelper.CheckVersionAndVariant(newUuid, 4);
                Check.That(generatedUuids).Not.Contains(newUuid);
                generatedUuids.Add(newUuid);
            }
        }
    }
}
