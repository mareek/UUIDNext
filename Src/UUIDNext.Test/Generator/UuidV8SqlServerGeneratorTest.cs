using System;
using System.Data.SqlTypes;
using System.Linq;
using NFluent;
using UUIDNext.Generator;
using UUIDNext.Tools;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV8SqlServerGeneratorTest : UuidTimestampGeneratorBaseTest<UuidV8SqlServerGenerator>
    {
        protected override byte Version => 8;

        protected override TimeSpan TimestampGranularity => TimeSpan.FromMilliseconds(1);

        protected override int SequenceBitSize => 14;

        protected override (long timestamp, int sequence) DecodeUuid(Guid uuid)
            => UuidDecoder.DecodeUuidV8ForSqlServer(uuid);

        protected override Guid NewUuid(UuidV8SqlServerGenerator generator) => generator.New();

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
