using BenchmarkDotNet.Attributes;

namespace UUIDNext.Benchmarks;

[MemoryDiagnoser(false)]
public class CacheBench
{
    [Params(128, 256, 512, 1024, 2048, 4096, 8192)]
    public int CacheSize { get; set; }

    [Benchmark]
    public void BestCase()
    {
        Generator.UuidV7FromSpecificDateGenerator uuidV7Generator = new(CacheSize);
        DateTimeOffset date = new(new(2000, 1, 1));
        for (int i = 0; i < 16384; i++)
        {
            uuidV7Generator.New(date);
        }
    }

    [Benchmark]
    public void InBetweenCase()
    {
        Generator.UuidV7FromSpecificDateGenerator uuidV7Generator = new(CacheSize);
        DateTimeOffset date = new(new(2000, 1, 1));
        for (int i = 0; i < 16384; i++)
        {
            uuidV7Generator.New(date.AddSeconds(i % (CacheSize / 2)));
        }
    }

    [Benchmark]
    public void WorstCase()
    {
        Generator.UuidV7FromSpecificDateGenerator uuidV7Generator = new(CacheSize);
        DateTimeOffset date = new(new(2000, 1, 1));
        for (int i = 0; i < 16384; i++)
        {
            uuidV7Generator.New(date.AddSeconds(i));
        }
    }
}
