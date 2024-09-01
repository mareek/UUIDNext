using BenchmarkDotNet.Attributes;

namespace UUIDNext.Benchmarks;

[MemoryDiagnoser(false)]
public class UuidBench
{
    private static readonly Guid urlNamespaceId = Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
    private static readonly Generator.UuidV8SqlServerGenerator uuidV8Generator = new();

    [Benchmark]
    public Guid NewGuid() => Guid.NewGuid();

    [Benchmark]
    public Guid CreateVersion7() => Guid.CreateVersion7();

    [Benchmark]
    public Guid NewUuidV4() => Uuid.NewRandom();

    [Benchmark]
    public Guid NewUuidV5() => Uuid.NewNameBased(urlNamespaceId, "http://www.example.com");

    [Benchmark]
    public Guid NewUuidV7() => Uuid.NewSequential();

    [Benchmark]
    public Guid NewUuidV7Static() => Uuid.NewSequential_static();

    [Benchmark]
    public Guid NewUuidV7Simple() => Uuid.NewSequential_simple();
    [Benchmark]
    public Guid NewUuidV7SkipInit() => Uuid.NewSequential_skipInit();

    [Benchmark]
    public Guid NewUuidV8() => uuidV8Generator.New();
}
