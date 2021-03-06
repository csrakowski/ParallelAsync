﻿using System;
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
    public class ParallelAsyncBenchmarks_IAsyncEnumerable
    {
        private const int NumberOfItemsInCollection = 10000;
        private const int NumberOfItemsInCollection2 = 10;
        private const int DelayTime = 1;

        private readonly IAsyncEnumerable<int> InputNumbers;
        private readonly IAsyncEnumerable<int> InputNumbers2;

        public ParallelAsyncBenchmarks_IAsyncEnumerable()
        {
            InputNumbers = Enumerable.Range(0, NumberOfItemsInCollection).ToList().AsAsyncEnumerable();
            InputNumbers2 = Enumerable.Range(0, NumberOfItemsInCollection2).ToList().AsAsyncEnumerable();
        }

        [Params(1, 4, 8)]
        public int MaxBatchSize { get; set; }

        [Params(false, true)]
        public bool AllowOutOfOrder { get; set; }

        [Benchmark, BenchmarkCategory("JustAddOne", "QuickRun")]
        public Task JustAddOne()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, Func_JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("JustAddOne", "QuickRun")]
        public Task JustAddOne_WithCancellation()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, Func_JustAddOne_WithCancellation, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask", "QuickRun")]
        public Task ReturnTaskCompletedTask()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, Func_ReturnTaskCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ReturnTaskCompletedTask", "QuickRun")]
        public Task ReturnTaskCompletedTask_WithCancellation()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, Func_ReturnTaskCompletedTask_WithCancellation, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne_WithDelay")]
        public Task JustAddOne_WithDelay()
        {
            return ParallelAsync.ForEachAsync(InputNumbers2, Func_JustAddOne_WithDelay, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection2, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("JustAddOne_WithDelay")]
        public Task JustAddOne_WithCancellation_WithDelay()
        {
            return ParallelAsync.ForEachAsync(InputNumbers2, Func_JustAddOne_WithCancellation_WithDelay, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection2, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask_WithDelay")]
        public Task ReturnTaskCompletedTask_WithDelay()
        {
            return ParallelAsync.ForEachAsync(InputNumbers2, Func_ReturnTaskCompletedTask_WithDelay, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ReturnTaskCompletedTask_WithDelay")]
        public Task ReturnTaskCompletedTask_WithCancellation_WithDelay()
        {
            return ParallelAsync.ForEachAsync(InputNumbers2, Func_ReturnTaskCompletedTask_WithCancellation_WithDelay, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
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
