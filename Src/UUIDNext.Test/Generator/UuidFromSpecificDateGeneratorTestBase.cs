using System;
using NFluent;
using UUIDNext.Tools;
using Xunit;

namespace UUIDNext.Test.Generator;

public abstract class UuidFromSpecificDateGeneratorTestBase : UuidTimestampGeneratorBaseTest
{
    [Theory]
    [MemberData(nameof(InvalidDates))]
    public void CheckThatDateBeforeUnixEpochAreRejected(DateTimeOffset date)
    {
        var generator = NewGenerator();
        Check.ThatCode(() => NewUuid(generator, date)).Throws<ArgumentOutOfRangeException>();
    }

    public static object[][] InvalidDates
        => [[new DateTimeOffset(new(1900, 1, 1))], [DateTimeOffset.FromUnixTimeMilliseconds(-1)], [DateTimeOffset.MinValue]];

    [Fact]
    public void EnsureThatUuidsFromTheSameTimestampAreIncreasing()
    {
        UuidWithTimestampComparer comparer = new();
        DateTimeOffset date = new(new(2020, 2, 1));
        var generator = NewGenerator();
        Guid previousUuid = NewUuid(generator, date.AddMilliseconds(-1));
        for (int i = 0; i < 100; i++)
        {
            var uuid = NewUuid(generator, date);
            Check.That(comparer.Compare(previousUuid, uuid)).IsStrictlyLessThan(0);
            previousUuid = uuid;
        }
    }

    [Fact]
    public void EnsureThatUuidsFromMultipleTimestampAreIncreasing()
    {
        UuidWithTimestampComparer comparer = new();

        DateTimeOffset dateT = new(new(2028, 10, 12));
        DateTimeOffset dateA = new(new(2030, 5, 1));
        DateTimeOffset dateS = new(new(2037, 11, 24));

        var generator = NewGenerator();

        Guid previousUuidT = NewUuid(generator, dateT.AddMilliseconds(-1));
        Guid previousUuidA = NewUuid(generator, dateA.AddMilliseconds(-1));
        Guid previousUuidS = NewUuid(generator, dateS.AddMilliseconds(-1));

        for (int i = 0; i < 100; i++)
        {
            var uuidT = NewUuid(generator, dateT);
            var uuidA = NewUuid(generator, dateA);
            var uuidS = NewUuid(generator, dateS);

            Check.That(comparer.Compare(previousUuidT, uuidT)).IsStrictlyLessThan(0);
            Check.That(comparer.Compare(previousUuidA, uuidA)).IsStrictlyLessThan(0);
            Check.That(comparer.Compare(previousUuidS, uuidS)).IsStrictlyLessThan(0);

            previousUuidT = uuidT;
            previousUuidA = uuidA;
            previousUuidS = uuidS;
        }
    }

    [Fact]
    public void EnsureTimestampAndVersionAreAlwaysCorrect()
    {
        UuidWithTimestampComparer comparer = new();
        DateTime date = new(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var generator = NewGenerator();

        int overflowCount = 0;
        Guid previousUuid = NewUuid(generator, date.AddMilliseconds(-1));

        // we loop 10_000 times to make sure that the sequence of the UUID v7 will overflow at
        // least twice since the sequence is stored on 12 bits and its maximum value is thus 4095
        int iterationCount = (int)(GetSequenceMaxValue() * 2.5);
        for (int i = 0; i < iterationCount; i++)
        {
            var uuid = NewUuid(generator, date);
            Check.That(UuidDecoder.GetVersion(uuid)).Is(Version);
            var uuidDate = UuidTestHelper.DecodeDate(uuid);
            Check.That(uuidDate).Is(date);

            if (comparer.Compare(previousUuid, uuid) > 0)
                overflowCount++;

            previousUuid = uuid;
        }

        Check.That(overflowCount).IsGreaterOrEqualThan(2).And.IsLessOrEqualThan(5);
    }
}
