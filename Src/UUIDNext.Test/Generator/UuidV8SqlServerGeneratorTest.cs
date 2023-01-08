using System;
using System.Collections.Concurrent;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV8SqlServerGeneratorTest
    {
        [Fact]
        public void DumbTest()
        {
            var generator = new UuidV8SqlServerGenerator();
            ConcurrentBag<Guid> generatedUuids = new();
            Parallel.For(0, 1000, _ => generatedUuids.Add(generator.New()));

            Check.That(generatedUuids).ContainsNoDuplicateItem();

            foreach (var uuid in generatedUuids)
            {
                UuidTestHelper.CheckVersionAndVariant(uuid, 8);
            }
        }

        [Fact]
        public void TestOrderWithSqlGuid()
        {
            var generator = new UuidV8SqlServerGenerator();
            var testSet = generator.GetDatabaseTestSet(100);

            var testSetWithSqlGuid = testSet.Select(s => (s.expectedPosition, sqlUuid: new SqlGuid(s.uuid)))
                                            .OrderBy(s => s.sqlUuid)
                                            .ToArray();

            for (int i = 1; i < testSetWithSqlGuid.Length; i++)
            {
                var previousValue = testSetWithSqlGuid[i - 1];
                var currentValue = testSetWithSqlGuid[i];
                Check.That(currentValue.expectedPosition).IsStrictlyGreaterThan(previousValue.expectedPosition);
            }
        }
    }
}
