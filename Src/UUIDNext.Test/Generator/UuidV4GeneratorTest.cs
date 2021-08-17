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
            for (int i = 0; i < 1000; i++)
            {
                var newUuid = Uuid.NewV4();
                var strUuid = newUuid.ToString();
                Check.That(strUuid[14]).IsEqualTo('4');
                Check.That(strUuid[19]).IsOneOf('8', '9', 'a', 'b', 'A', 'B');
                Check.That(generatedUuids).Not.Contains(newUuid);
                generatedUuids.Add(newUuid);
            }
        }
    }
}
