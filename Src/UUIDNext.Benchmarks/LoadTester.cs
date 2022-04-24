using System.Diagnostics;

namespace UUIDNext.Benchmarks;

internal class LoadTester
{
    private readonly bool _parallel;
    private readonly string _version;
    private readonly TimeSpan _testDuration;

    public LoadTester(bool parallel, string version, TimeSpan testDuration)
    {
        _parallel = parallel;
        _version = version;
        _testDuration = testDuration;
    }

    public void LaunchLoadTest()
    {
        const int runLenght = 100_000;

        Func<Guid> funcUnderTest = GetFuncUnderTest(_version);

        var chrono = Stopwatch.StartNew();
        do
        {
            if (_parallel)
            {
                Parallel.For(0, runLenght, _ => funcUnderTest());
            }
            else
            {
                for (int i = 0; i < runLenght; i++)
                {
                    funcUnderTest();
                }
            }
        } while (chrono.Elapsed < _testDuration);
    }

    private static Func<Guid> GetFuncUnderTest(string version)
    {
        Guid urlNamespaceId = Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
        Generator.UuidV6Generator uuidV6Generator = new();

        return version switch
        {
            "4" or "U4" or "u4" => Uuid.NewRandom,
            "5" or "U5" or "u5" => () => Uuid.NewNameBased(urlNamespaceId, "http://www.example.com"),
            "6" or "U6" or "u6" => uuidV6Generator.New,
            "g" or "G" or "guid" or "GUID" => Guid.NewGuid,
            _ => Uuid.NewDatabaseFriendly,
        };
    }

    public static void LaunchFromCommandLine(string[] args)
    {
        const string defaultVersion = "7";
        const int defaultDurationInS = 10;

        string version = defaultVersion;
        TimeSpan duration = TimeSpan.FromSeconds(defaultDurationInS);
        bool parallel = false;

        for (int i = 1; i < args.Length; i++)
        {
            string arg = args[i];
            if (CheckArg(arg, "version"))
            {
                version = args.ElementAtOrDefault(i + 1) ?? defaultVersion;
                i += 1;
            }
            else if (CheckArg(arg, "duration"))
            {
                int durationInS = int.TryParse(args.ElementAtOrDefault(i + 1), out int val) ? val : defaultDurationInS;
                duration = TimeSpan.FromSeconds(durationInS);
                i += 1;
            }
            else if (CheckArg(arg, "parallel"))
            {
                parallel = true;
            }
        }

        LoadTester loadTester = new(parallel, version, duration);
        loadTester.LaunchLoadTest();

        static bool CheckArg(string arg, string argName)
        {
            return arg.Equals($"-{argName[0]}", StringComparison.OrdinalIgnoreCase)
                || arg.Equals($"--{argName}", StringComparison.OrdinalIgnoreCase);
        }
    }
}
