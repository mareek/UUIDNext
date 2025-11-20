using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;

namespace UUIDNext.Benchmarks;

[MemoryDiagnoser(false)]
[HideColumns(Column.Error, Column.StdDev, Column.RatioSD)]
public class UuidBench
{
    private const string ShortUrl = "http://www.example.com";
    private static readonly Guid urlNamespaceId = Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

    private static readonly string longUrl = $"{ShortUrl}/?token={GetHexString(1024)}";
    private static string GetHexString(int stringLength)
    {
        Random rng = new();
        byte[] buffer = new byte[stringLength / 2];
        rng.NextBytes(buffer);
        return BitConverter.ToString(buffer)
                           .Replace("-", "")
                           .ToLower();
    }

    private static readonly Generator.UuidV8SqlServerGenerator uuidV8Generator = new();
    private static readonly Generator.UuidV7FromSpecificDateGenerator uuidV7Generator = new();

    [Benchmark(Baseline = true)]
    public Guid NewGuid() => Guid.NewGuid();

    [Benchmark]
    public Guid NewUuidV4() => Uuid.NewRandom();

#if NET9_0_OR_GREATER
    [Benchmark()]
    public Guid CreateVersion7() => Guid.CreateVersion7();
#endif

    [Benchmark]
    public Guid NewUuidV7() => Uuid.NewSequential();

    [Benchmark]
    public Guid NewUuidV8() => uuidV8Generator.New();

    [Benchmark]
    public Guid NewUuidV7ArbitraryDate() => uuidV7Generator.New(DateTime.Today);

    [Benchmark]
    public Guid NewUuidV5_short() => Uuid.NewNameBased(urlNamespaceId, ShortUrl);

    [Benchmark]
    public Guid NewUuidV5_long() => Uuid.NewNameBased(urlNamespaceId, longUrl);
}
