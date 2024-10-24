using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV8SqlServerGeneratorTest : UuidTimestampGeneratorBaseTest
    {
        protected override byte Version => 8;

        protected override int SequenceBitSize => 14;

        protected override Guid NewUuid(object generator) => ((UuidV8SqlServerGenerator)generator).New();

        protected override object NewGenerator() => new UuidV8SqlServerGenerator();

        [Fact]
        public void TestOrderWithSqlGuid()
        {
            var generator = new UuidV8SqlServerGenerator();
            var testSet = UuidTestHelper.GetDatabaseTestSet(generator, 100);

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

        [Fact]
        public void CheckThatSequenceCounterIsCorrectlySeeded()
        {
            const string digits = "0123456789abcdef";
            const int testCount = 1_000;

            UuidV8SqlServerGenerator generator = new();

            Dictionary<char, int> charCount17 = digits.ToDictionary(c => c, _ => 0);
            Dictionary<char, int> charCount18 = digits.ToDictionary(c => c, _ => 0);

            DateTimeOffset date = new(new(2000, 1, 1));
            for (int i = 0; i < testCount; i++)
            {
                var newUuid = UuidTestHelper.New(generator, date.AddSeconds(i));
                char seventeenthDigit = newUuid.ToString()[19];
                charCount17[seventeenthDigit] += 1;
                char eighteenthDigit = newUuid.ToString()[20];
                charCount18[eighteenthDigit] += 1;
            }

            // the 17th digit is constrained by the variant bits and some implementation details to be 8 or 9 at seed 
            Check.That(charCount17['8']).IsGreaterOrEqualThan(testCount / 4);
            Check.That(charCount17['9']).IsGreaterOrEqualThan(testCount / 4);

            // On the other hand, the 18th digit can be any hexadecila digit so we check that each digit appear a significant ammount of time
            int expectedOccurenceCount = testCount / (digits.Length * 2);
            foreach (var digit in digits)
            {
                Check.That(charCount18[digit]).IsGreaterOrEqualThan(expectedOccurenceCount);
            }
        }
    }
}
