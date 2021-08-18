using System;
using System.Collections.Generic;
using NFluent;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV4GeneratorTest
    {
        [Fact]
        public void UuidV4DumbTest()
        {
            HashSet<Guid> generatedUuids = new();
            for (int i = 0; i < 100; i++)
            {
                var newUuid = Uuid.NewV4();
                UuidTestHelper.CheckVersionAndVariant(newUuid, 4);
                Check.That(generatedUuids).Not.Contains(newUuid);
                generatedUuids.Add(newUuid);
            }
        }
    }
}
