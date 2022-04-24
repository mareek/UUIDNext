namespace UUIDNext.Benchmarks;

internal class Doc
{
    public static string CommandLine =
@$"usage: 
UUIDNext.Benchmarks bench
    Run benchmarkDotnet on UUIDNext

UUIDNext.Benchmarks load [<options>]
    Run a load test of UUID Generation to ease the use of a profiler

    -v, --version <version>     UUID version to be tested (default is 7) 
    -p, --parallel              Run the UUID generation in parallel
    -d, --duration <time in s>  duration of the test run (default is 10s)";
}
