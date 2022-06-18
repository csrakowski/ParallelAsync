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
using CSRakowski.Parallel.Helpers;
using CSRakowski.AsyncStreamsPreparations;

namespace CSRakowski.Parallel.Benchmarks
{
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    [SimpleJob(RuntimeMoniker.Net48, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31, baseline: false)]
    [SimpleJob(RuntimeMoniker.Net50, baseline: false)]
    [SimpleJob(RuntimeMoniker.Net60, baseline: false)]
    public class ParallelAsyncBenchmarks_AsyncStreams
    {
        private const int NumberOfItemsInCollection = 10000;

        private readonly IEnumerable<int> InputNumbers;
        private readonly IAsyncEnumerable<int> InputNumbersAsync;

        public ParallelAsyncBenchmarks_AsyncStreams()
        {
            InputNumbers = Enumerable.Range(0, NumberOfItemsInCollection).ToList();
            InputNumbersAsync = InputNumbers.AsAsyncEnumerable();
        }

        [Params(1, 4, 8)]
        public int MaxBatchSize { get; set; }

        [Params(false, true)]
        public bool AllowOutOfOrder { get; set; }

        [Benchmark(Baseline = true), BenchmarkCategory("ForEachAsync", "IEnumerable")]
        public async Task<int> IEnumerable_ForEachAsync()
        {
            var result = await ParallelAsync.ForEachAsync(InputNumbers, TestFunctions.JustAddOne_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
            return result.Count();
        }

        [Benchmark, BenchmarkCategory("ForEachAsync", "IAsyncEnumerable")]
        public async Task<int> IAsyncEnumerable_ForEachAsync()
        {
            var result = await ParallelAsync.ForEachAsync(InputNumbersAsync, TestFunctions.JustAddOne_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
            return result.Count();
        }

        #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER

        [Benchmark(Baseline = true), BenchmarkCategory("ForEachAsyncStream", "IEnumerable")]
        public async Task<int> IEnumerable_ForEachAsyncStream()
        {
            int count = 0;

            await foreach (var r in ParallelAsync.ForEachAsyncStream(InputNumbers, TestFunctions.JustAddOne_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None))
            {
                count++;
            }

            return count;
        }

        [Benchmark, BenchmarkCategory("ForEachAsyncStream", "IAsyncEnumerable")]
        public async Task<int> IAsyncEnumerable_ForEachAsyncStream()
        {
            int count = 0;

            await foreach (var r in ParallelAsync.ForEachAsyncStream(InputNumbersAsync, TestFunctions.JustAddOne_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None))
            {
                count++;
            }

            return count;
        }

        #endif //NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER

    }
}
