using System;
using Xunit;
using UUIDNext.Generator;
using System.Collections.Generic;
using NFluent;

namespace UUIDNext.Test.Generator
{
    public class UuidV4GeneratorTest
    {
        [Fact]
        public void DumbTest()
        {
            UuidV4Generator generator = new();
            HashSet<Guid> generatedUuids = new();
            for (int i = 0; i < 1000; i++)
            {
                var newUuid = generator.New();
                var strUuid = newUuid.ToString();
                Check.That(strUuid[14]).IsEqualTo('4');
                Check.That(strUuid[19]).IsOneOf('8', '9', 'a', 'b', 'A', 'B');
                Check.That(generatedUuids).Not.Contains(newUuid);
                generatedUuids.Add(newUuid);
            }
        }
    }
}
