using System;
using System.Linq;
using NFluent;
using UUIDNext.Generator;
using UUIDNext.Tools;
using Xunit;

namespace UUIDNext.Test.Tools;

public class UUIDToolkitTest
{
    GuidComparer _comparer = new();

    [Fact]
    public void CheckThatUuidV7FromCurrentDateTimeIsCoherentWithNewSequential()
    {
        const int testCount = 10_000;
        int errorCount = 0;
        for (int i = 0; i < testCount; i++)
        {
            var uuidFromSequential = Uuid.NewSequential();
            var uuidFromToolkit = UuidToolkit.CreateUuidV7FromSpecificDate(DateTimeOffset.Now);

            if (0 <= _comparer.Compare(uuidFromSequential, uuidFromToolkit))
                errorCount += 1;
        }

        // We check that 99.5% of the call get forwarded to Uuid.NewSequential() as we can never reach 100% acuracy
        Check.That(errorCount * 100.0 / testCount).IsStrictlyLessThan(0.5);
    }

    [Fact]
    public void CheckThatUuidV7DateOrderIsRespected()
    {
        var uuidBeforeNow = UuidToolkit.CreateUuidV7FromSpecificDate(DateTimeOffset.UtcNow.AddMilliseconds(-2));
        var uuidNow = Uuid.NewSequential();
        var uuidAfterNow = UuidToolkit.CreateUuidV7FromSpecificDate(DateTimeOffset.UtcNow.AddMilliseconds(2));

        Check.That(_comparer.Compare(uuidBeforeNow, uuidNow)).IsStrictlyNegative();
        Check.That(_comparer.Compare(uuidNow, uuidAfterNow)).IsStrictlyNegative();
    }
}
