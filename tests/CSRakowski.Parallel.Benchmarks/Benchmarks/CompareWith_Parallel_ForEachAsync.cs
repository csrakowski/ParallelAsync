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
using System.Collections.ObjectModel;

namespace CSRakowski.Parallel.Benchmarks
{
#if NET6_0_OR_GREATER

    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    [SimpleJob(RuntimeMoniker.Net60, baseline: true)]
    public class CompareWith_Parallel_ForEachAsync
    {
        private const int NumberOfItemsInCollection = 10000;

        private readonly IEnumerable<int> InputNumbers;
        private readonly IAsyncEnumerable<int> InputNumbersAsync;

        public CompareWith_Parallel_ForEachAsync()
        {
            InputNumbers = Enumerable.Range(0, NumberOfItemsInCollection).ToList();
            InputNumbersAsync = InputNumbers.AsAsyncEnumerable();
        }

        [Params(1, 4, 8)]
        public int MaxBatchSize { get; set; }

        [Params(false, true)]
        public bool UseFrameworkImplementation { get; set; }

        [Benchmark, BenchmarkCategory("Compute_Double", "IEnumerable")]
        public async Task IEnumerable_Compute_Double()
        {
            if (UseFrameworkImplementation)
            {
                var concurrentResult = new System.Collections.Concurrent.ConcurrentBag<double>();

                var options = new System.Threading.Tasks.ParallelOptions
                {
                    CancellationToken = CancellationToken.None,
                    MaxDegreeOfParallelism = MaxBatchSize
                };

                await System.Threading.Tasks.Parallel.ForEachAsync<int>(InputNumbers, options, async (i, ct) =>
                {
                    var r = await TestFunctions.Compute_Double(i).ConfigureAwait(false);
                    concurrentResult.Add(r);
                }).ConfigureAwait(false);
            }
            else
            {
                var total = await ParallelAsync.ForEachAsync(collection: InputNumbers,
                                                             func: TestFunctions.Compute_Double,
                                                             maxBatchSize: MaxBatchSize,
                                                             allowOutOfOrderProcessing: true,
                                                             estimatedResultSize: NumberOfItemsInCollection,
                                                             cancellationToken: CancellationToken.None)
                                                .ConfigureAwait(false);
            }
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask", "IEnumerable")]
        public async Task IEnumerable_ReturnTaskCompletedTask()
        {
            if (UseFrameworkImplementation)
            {
                var concurrentResult = new System.Collections.Concurrent.ConcurrentBag<int>();

                var options = new System.Threading.Tasks.ParallelOptions
                {
                    CancellationToken = CancellationToken.None,
                    MaxDegreeOfParallelism = MaxBatchSize
                };

                await System.Threading.Tasks.Parallel.ForEachAsync<int>(InputNumbers, options, async (i, ct) =>
                {
                    await TestFunctions.ReturnCompletedTask(i).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            else
            {
                await ParallelAsync.ForEachAsync(collection: InputNumbers,
                                                 func: TestFunctions.ReturnCompletedTask,
                                                 maxBatchSize: MaxBatchSize,
                                                 allowOutOfOrderProcessing: true,
                                                 cancellationToken: CancellationToken.None)
                                    .ConfigureAwait(false);
            }
        }

        [Benchmark, BenchmarkCategory("Compute_Double", "IAsyncEnumerable")]
        public async Task IAsyncEnumerable_Compute_Double()
        {
            if (UseFrameworkImplementation)
            {
                var concurrentResult = new System.Collections.Concurrent.ConcurrentBag<double>();

                var options = new System.Threading.Tasks.ParallelOptions
                {
                    CancellationToken = CancellationToken.None,
                    MaxDegreeOfParallelism = MaxBatchSize
                };

                await System.Threading.Tasks.Parallel.ForEachAsync<int>(InputNumbersAsync, options, async (i, ct) =>
                {
                    var r = await TestFunctions.Compute_Double(i).ConfigureAwait(false);
                    concurrentResult.Add(r);
                }).ConfigureAwait(false);
            }
            else
            {
                var total = await ParallelAsync.ForEachAsync(collection: InputNumbersAsync,
                                                             func: TestFunctions.Compute_Double,
                                                             maxBatchSize: MaxBatchSize,
                                                             allowOutOfOrderProcessing: true,
                                                             estimatedResultSize: NumberOfItemsInCollection,
                                                             cancellationToken: CancellationToken.None)
                                                .ConfigureAwait(false);
            }
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask", "IAsyncEnumerable")]
        public async Task IAsyncEnumerable_ReturnTaskCompletedTask()
        {
            if (UseFrameworkImplementation)
            {
                var concurrentResult = new System.Collections.Concurrent.ConcurrentBag<int>();

                var options = new System.Threading.Tasks.ParallelOptions
                {
                    CancellationToken = CancellationToken.None,
                    MaxDegreeOfParallelism = MaxBatchSize
                };

                await System.Threading.Tasks.Parallel.ForEachAsync<int>(InputNumbersAsync, options, async (i, ct) =>
                {
                    await TestFunctions.ReturnCompletedTask(i).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            else
            {
                await ParallelAsync.ForEachAsync(collection: InputNumbersAsync,
                                                 func: TestFunctions.ReturnCompletedTask,
                                                 maxBatchSize: MaxBatchSize,
                                                 allowOutOfOrderProcessing: true,
                                                 cancellationToken: CancellationToken.None)
                                    .ConfigureAwait(false);
            }
        }
    }

#endif

}
