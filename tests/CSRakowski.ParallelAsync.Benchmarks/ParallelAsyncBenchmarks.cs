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

namespace CSRakowski.Parallel.Benchmarks
{
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

        [Params(1, 2, 4, 8)]
        public int MaxBatchSize { get; set; }

        [Params(false, true)]
        public bool AllowOutOfOrder { get; set; }

        [Benchmark]
        public async Task JustAddOne()
        {
            var batchResults = await ParallelAsync.ForEachAsync(InputNumbers, Func_JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task JustAddOne_WithCancellation()
        {
            var batchResults = await ParallelAsync.ForEachAsync(InputNumbers, Func_JustAddOne_WithCancellation, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task ReturnTaskCompletedTask()
        {
            await ParallelAsync.ForEachAsync(InputNumbers, Func_ReturnTaskCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task ReturnTaskCompletedTask_WithCancellation()
        {
            await ParallelAsync.ForEachAsync(InputNumbers, Func_ReturnTaskCompletedTask_WithCancellation, MaxBatchSize, AllowOutOfOrder, CancellationToken.None).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task JustAddOne_WithDelay()
        {
            var batchResults = await ParallelAsync.ForEachAsync(InputNumbers2, Func_JustAddOne_WithDelay, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection2, CancellationToken.None).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task JustAddOne_WithCancellation_WithDelay()
        {
            var batchResults = await ParallelAsync.ForEachAsync(InputNumbers2, Func_JustAddOne_WithCancellation_WithDelay, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection2, CancellationToken.None).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task ReturnTaskCompletedTask_WithDelay()
        {
            await ParallelAsync.ForEachAsync(InputNumbers2, Func_ReturnTaskCompletedTask_WithDelay, MaxBatchSize, AllowOutOfOrder, CancellationToken.None).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task ReturnTaskCompletedTask_WithCancellation_WithDelay()
        {
            await ParallelAsync.ForEachAsync(InputNumbers2, Func_ReturnTaskCompletedTask_WithCancellation_WithDelay, MaxBatchSize, AllowOutOfOrder, CancellationToken.None).ConfigureAwait(false);
        }

        #region Delegates

        private static Task<int> Func_JustAddOne(int number)
        {
            return Task.FromResult(number + 1);
        }

        private static Task<int> Func_JustAddOne_WithCancellation(int number, CancellationToken cancellationToken)
        {
            return Task.FromResult(number + 1);
        }

        private static Task Func_ReturnTaskCompletedTask(int number)
        {
            return Task.CompletedTask;
        }

        private static Task Func_ReturnTaskCompletedTask_WithCancellation(int number, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private static async Task<int> Func_JustAddOne_WithDelay(int number)
        {
            await Task.Delay(DelayTime).ConfigureAwait(false);
            return (number + 1);
        }

        private static async Task<int> Func_JustAddOne_WithCancellation_WithDelay(int number, CancellationToken cancellationToken)
        {
            await Task.Delay(DelayTime).ConfigureAwait(false);
            return (number + 1);
        }

        private static Task Func_ReturnTaskCompletedTask_WithDelay(int number)
        {
            return Task.Delay(DelayTime);
        }

        private static Task Func_ReturnTaskCompletedTask_WithCancellation_WithDelay(int number, CancellationToken cancellationToken)
        {
            return Task.Delay(DelayTime);
        }

        #endregion Delegates

    }
}
