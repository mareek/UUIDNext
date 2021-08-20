using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV7GeneratorTest
    {
        [Fact]
        public void DumbTest()
        {
            ConcurrentBag<Guid> generatedUuids = new();
            UuidV7Generator generator = new();

            Parallel.For(0, 100, i => generatedUuids.Add(generator.New()));

            Check.That(generatedUuids).ContainsNoDuplicateItem();

            foreach (var uuid in generatedUuids)
            {
                UuidTestHelper.CheckVersionAndVariant(uuid, 7);
            }
        }
    }
}
