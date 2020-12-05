using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace CSRakowski.Parallel.Benchmarks
{
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod, BenchmarkLogicalGroupRule.ByParams)]
    [SimpleJob(RuntimeMoniker.Net48, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31, baseline: false)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50, baseline: false)]
    public class ParallelAsyncTestBenchmarks
    {
        private const int NumberOfItemsInCollection = 10000;

        private readonly List<int> InputNumbers;

        public ParallelAsyncTestBenchmarks()
        {
            InputNumbers = Enumerable.Range(0, NumberOfItemsInCollection).ToList();
        }

        [Params(4, 8)]
        public int MaxBatchSize { get; set; }

        [Params(false, true)]
        public bool AllowOutOfOrder { get; set; }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.ReturnCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }
    }
}
