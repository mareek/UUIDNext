using System;
using System.Threading.Tasks;
using NFluent;
using Xunit;

namespace UUIDNext.Test.Generator;

public abstract class UuidTimestampWithOverflowGeneratorBaseTest : UuidTimestampGeneratorBaseTest
{
    protected override Guid NewUuid(object generator, DateTimeOffset date)
        => UuidTestHelper.New(generator, date);

    [Fact]
    public void TestSequenceOverflow()
    {
        var generator = NewGenerator();
        var date = DateTime.UtcNow.Date;

        var firstUuid = UuidTestHelper.New(generator, date);
        var firstDate = UuidTestHelper.DecodeDate(firstUuid);
        var firstSequence = UuidTestHelper.DecodeSequence(firstUuid);

        Check.That(firstSequence).IsLessOrEqualThan(GetSeedMaxValue());

        Parallel.For(0, GetSequenceMaxValue() - firstSequence, _ => UuidTestHelper.New(generator, date));

        var lastUuid = UuidTestHelper.New(generator, date);
        var lastDate = UuidTestHelper.DecodeDate(lastUuid);
        var lastSequence = UuidTestHelper.DecodeSequence(lastUuid);
        Check.That(lastSequence).IsLessOrEqualThan(GetSeedMaxValue());
        Check.That(lastDate).IsEqualTo(firstDate.AddMilliseconds(1));
    }

    [Fact]
    public void TestSequenceOverflowWithOffset()
    {
        var generator = NewGenerator();
        var date = DateTime.UtcNow.Date;

        //setup an offset by generating an uuid into the future
        UuidTestHelper.New(generator, date.AddSeconds(1));

        var firstUuid = UuidTestHelper.New(generator, date);
        var firstDate = UuidTestHelper.DecodeDate(firstUuid);
        var firstSequence = UuidTestHelper.DecodeSequence(firstUuid);

        Check.That(firstSequence).IsLessOrEqualThan(GetSeedMaxValue());

        Parallel.For(0, GetSequenceMaxValue() - firstSequence, _ => UuidTestHelper.New(generator, date));

        var lastUuid = UuidTestHelper.New(generator, date);
        var lastDate = UuidTestHelper.DecodeDate(lastUuid);
        var lastSequence = UuidTestHelper.DecodeSequence(lastUuid);
        Check.That(lastSequence).IsLessOrEqualThan(GetSeedMaxValue());
        Check.That(lastDate).IsEqualTo(firstDate.AddMilliseconds(1));
    }
}
