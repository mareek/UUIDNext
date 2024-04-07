using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUIDNext.Demo;

internal static class StatsHelper
{
    public static TimeSpan[] ComputeAverage(TimeSpan[] inputData, int outputSize)
    {
        var chunckSize = inputData.Length / outputSize;
        return inputData.Chunk(chunckSize)
                        .Select(chunk => chunk.Select(e => e.TotalMilliseconds).Average())
                        .Select(TimeSpan.FromMilliseconds)
                        .ToArray();
    }

    public static TimeSpan[] ComputeMedian(TimeSpan[] inputData, int outputSize)
    {
        var chunckSize = inputData.Length / outputSize;
        return inputData.Chunk(chunckSize)
                        .Select(chunk => chunk.OrderBy(e => e.TotalMilliseconds).Skip(chunk.Length / 2).First())
                        .ToArray();
    }
}
