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
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    [SimpleJob(RuntimeMoniker.Net47, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp30, baseline: false)]
    public class ParallelAsyncBenchmarks
    {
        private const int NumberOfItemsInCollection = 10000;
        private const int NumberOfItemsInCollection2 = 10;
        private const int DelayTime = 1;

        private readonly List<int> InputNumbers;
        private readonly List<int> InputNumbers2;

        public ParallelAsyncBenchmarks()
        {
            InputNumbers = Enumerable.Range(0, NumberOfItemsInCollection).ToList();
            InputNumbers2 = Enumerable.Range(0, NumberOfItemsInCollection2).ToList();
        }

        [GlobalSetup]
        public void ConfigureTests()
        {
            TestFunctions.DelayTime = DelayTime;
        }

        [Params(1, 4, 8)]
        public int MaxBatchSize { get; set; }

        [Params(false, true)]
        public bool AllowOutOfOrder { get; set; }

        [Benchmark, BenchmarkCategory("JustAddOne", "QuickRun")]
        public Task JustAddOne()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("JustAddOne", "QuickRun")]
        public Task JustAddOne_WithCancellation()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.JustAddOne_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask", "QuickRun")]
        public Task ReturnTaskCompletedTask()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.ReturnCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ReturnTaskCompletedTask", "QuickRun")]
        public Task ReturnTaskCompletedTask_WithCancellation()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.ReturnCompletedTask_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne_WithDelay")]
        public Task JustAddOne_WithDelay()
        {
            return ParallelAsync.ForEachAsync(InputNumbers2, TestFunctions.JustAddOne_WithDelay, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection2, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("JustAddOne_WithDelay")]
        public Task JustAddOne_WithCancellation_WithDelay()
        {
            return ParallelAsync.ForEachAsync(InputNumbers2, TestFunctions.JustAddOne_WithCancellationToken_WithDelay, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection2, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask_WithDelay")]
        public Task ReturnTaskCompletedTask_WithDelay()
        {
            return ParallelAsync.ForEachAsync(InputNumbers2, TestFunctions.ReturnCompletedTask_WithDelay, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ReturnTaskCompletedTask_WithDelay")]
        public Task ReturnTaskCompletedTask_WithCancellation_WithDelay()
        {
            return ParallelAsync.ForEachAsync(InputNumbers2, TestFunctions.ReturnCompletedTask_WithCancellationToken_WithDelay, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }
    }
}
