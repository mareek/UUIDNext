﻿using BenchmarkDotNet.Running;
using UUIDNext.Benchmarks;

var firstArg = args.FirstOrDefault();
switch (firstArg)
{
    case "bench":
        BenchmarkRunner.Run<UuidBench>();
        break;
    case "qdbench":
        BenchmarkRunner.Run<QDCacheBench>();
        break;
    case "load":
        LoadTester.LaunchFromCommandLine(args);
        break;
    default:
        Console.WriteLine(Doc.CommandLine);
        break;
}
