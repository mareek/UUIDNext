using System;
using NFluent;
using UUIDNext.Generator;
using UUIDNext.Tools;
using Xunit;

namespace UUIDNext.Test.Generator;

public class UuidV7FromArbitraryDateGeneratorTest
{
    GuidComparer _comparer = new();

    [Fact]
    public void EnsureTimestampAndVersionAreAlwaysCorrect()
    {
        DateTimeOffset date = new(new(2020, 1, 1));
        UuidV7FromArbitraryDateGenerator generator = new();

        int overflowCount = 0;
        Guid previousUuid = Uuid.Nil;

        // we loop 10_000 times to make sure that the sequence of the UUID v7 will overflow at
        // least twice since the sequence is stored on 12 bits and its maximum value is thus 4095
        for (int i = 0; i < 10_000; i++)
        {
            var uuid = generator.New(date);
            Check.That(UuidDecoder.GetVersion(uuid)).Is(7);
            var (timestamp, sequence) = UuidDecoder.DecodeUuidV7(uuid);
            Check.That(timestamp).Is(date.ToUnixTimeMilliseconds());

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
        UuidV7FromArbitraryDateGenerator generator = new();
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

        UuidV7FromArbitraryDateGenerator generator = new();

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
