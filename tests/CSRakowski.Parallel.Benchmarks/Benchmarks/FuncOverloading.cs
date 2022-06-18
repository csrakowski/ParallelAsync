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
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory, BenchmarkLogicalGroupRule.ByParams)]
    [SimpleJob(RuntimeMoniker.Net48, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31, baseline: false)]
    [SimpleJob(RuntimeMoniker.Net50, baseline: false)]
    [SimpleJob(RuntimeMoniker.Net60, baseline: false)]
    public class FuncOverloading
    {
        private const int NumberOfItemsInCollection = 10000;

        private readonly List<int> InputNumbers;

        public FuncOverloading()
        {
            InputNumbers = Enumerable.Range(0, NumberOfItemsInCollection).ToList();
        }

        [Params(1, 8)]
        public int MaxBatchSize { get; set; }

        [Params(false, true)]
        public bool AllowOutOfOrder { get; set; }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_Wrapped()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_NotWrapped()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.JustAddOne_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("CompletedTask")]
        public Task CompletedTask_Wrapped()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.ReturnCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("CompletedTask")]
        public Task CompletedTask_NotWrapped()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.ReturnCompletedTask_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }
    }
}
