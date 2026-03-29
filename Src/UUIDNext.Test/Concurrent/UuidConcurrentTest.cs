using System;
using System.Threading;
using NFluent;
using UUIDNext.Tools;
using Xunit;

namespace UUIDNext.Test.Concurrent;

public class UuidConcurrentTest
{
    [Theory]
    [InlineData(8, 16_384)]
    public void SpecificDateStressTest(int threadCount, int dateCount)
    {
        int exceptionCount = 0;
        DateTimeOffset baseDate = new(new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
            threads[i] = new(Action);

        for (int i = 0; i < threadCount; i++)
            threads[i].Start();

        for (int i = 0; i < threadCount; i++)
            threads[i].Join();

        Check.That(exceptionCount).IsZero();

        void Action()
        {
            for (int dateOffset = 0; dateOffset < dateCount; dateOffset++)
                try
                {
                    var date = baseDate.AddMinutes(dateOffset);
                    var uuid = UuidToolkit.CreateUuidV7FromSpecificDate(date);
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref exceptionCount);
                    return;
                }
        }
    }
}
