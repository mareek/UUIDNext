using System;
using NFluent;
using UUIDNext.Generator;
using UUIDNext.Tools;
using Xunit;

namespace UUIDNext.Test.Generator;

public class UuidV7FromSpecificDateGeneratorTest
{
    GuidComparer _comparer = new();

    [Theory]
    [MemberData(nameof(InvalidDates))]
    public void CheckThatDateBeforeUnixEpochAreRejected(DateTimeOffset date)
    {
        UuidV7FromSpecificDateGenerator generator = new();
        Check.ThatCode(() => generator.New(date)).Throws<ArgumentOutOfRangeException>();
    }

    public static object[][] InvalidDates
        => [[new DateTimeOffset(new(1900, 1, 1))], [DateTimeOffset.FromUnixTimeMilliseconds(-1)], [DateTimeOffset.MinValue]];

    [Fact]
    public void EnsureTimestampAndVersionAreAlwaysCorrect()
    {
        DateTime date = new(2020, 1, 1, 0,0,0, DateTimeKind.Utc);
        UuidV7FromSpecificDateGenerator generator = new();

        int overflowCount = 0;
        Guid previousUuid = Uuid.Nil;

        // we loop 10_000 times to make sure that the sequence of the UUID v7 will overflow at
        // least twice since the sequence is stored on 12 bits and its maximum value is thus 4095
        for (int i = 0; i < 10_000; i++)
        {
            var uuid = generator.New(date);
            Check.That(UuidDecoder.GetVersion(uuid)).Is(7);
            var uuidDate = UuidTestHelper.DecodeDate(uuid);
            Check.That(uuidDate).Is(date);

            if (_comparer.Compare(previousUuid, uuid) > 0)
                overflowCount++;

            previousUuid = uuid;
        }

        Check.That(overflowCount).IsGreaterOrEqualThan(2).And.IsLessOrEqualThan(5);
    }

    [Fact]
    public void EnsureThatUuidsFromTheSameTimestampAreIncreasing()
    {
        DateTimeOffset date = new(new(2020, 1, 1));
        UuidV7FromSpecificDateGenerator generator = new();
        Guid previousUuid = Uuid.Nil;
        for (int i = 0; i < 100; i++)
        {
            var uuid = generator.New(date);
            Check.That(_comparer.Compare(previousUuid, uuid)).IsStrictlyLessThan(0);
            previousUuid = uuid;
        }
    }

    [Fact]
    public void EnsureThatUuidsFromMultipleTimestampAreIncreasing()
    {
        DateTimeOffset dateT = new(new(2008, 10, 12));
        DateTimeOffset dateA = new(new(2010, 5, 1));
        DateTimeOffset dateS = new(new(2017, 11, 24));

        UuidV7FromSpecificDateGenerator generator = new();

        Guid previousUuidT = Uuid.Nil;
        Guid previousUuidA = Uuid.Nil;
        Guid previousUuidS = Uuid.Nil;

        for (int i = 0; i < 100; i++)
        {
            var uuidT = generator.New(dateT);
            var uuidA = generator.New(dateA);
            var uuidS = generator.New(dateS);

            Check.That(_comparer.Compare(previousUuidT, uuidT)).IsStrictlyLessThan(0);
            Check.That(_comparer.Compare(previousUuidA, uuidA)).IsStrictlyLessThan(0);
            Check.That(_comparer.Compare(previousUuidS, uuidS)).IsStrictlyLessThan(0);

            previousUuidT = uuidT;
            previousUuidA = uuidA;
            previousUuidS = uuidS;
        }
    }
}
