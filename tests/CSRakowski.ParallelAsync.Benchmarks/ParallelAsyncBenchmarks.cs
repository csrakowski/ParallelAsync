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

        private readonly List<int> InputNumbers;

        public ParallelAsyncBenchmarks()
        {
            InputNumbers = Enumerable.Range(0, NumberOfItemsInCollection).ToList();
        }

        #region Benchmarks: JustAddOne

        [Benchmark]
        public async Task Run_JustAddOne_Unbatched_AsOrdered()
        {
            await Run_JustAddOne_With(maxBatchSize: 1, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_JustAddOne_Unbatched_OutOfOrder()
        {
            await Run_JustAddOne_With(maxBatchSize: 1, outOfOrder: true);
        }

        [Benchmark]
        public async Task Run_JustAddOne_MaxBatch_2_AsOrdered()
        {
            await Run_JustAddOne_With(maxBatchSize: 2, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_JustAddOne_MaxBatch_2_OutOfOrder()
        {
            await Run_JustAddOne_With(maxBatchSize: 2, outOfOrder: true);
        }

        [Benchmark]
        public async Task Run_JustAddOne_MaxBatch_4_AsOrdered()
        {
            await Run_JustAddOne_With(maxBatchSize: 4, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_JustAddOne_MaxBatch_4_OutOfOrder()
        {
            await Run_JustAddOne_With(maxBatchSize: 4, outOfOrder: true);
        }

        #endregion Benchmarks: JustAddOne

        #region Benchmarks: JustAddOneWithCancellation

        [Benchmark]
        public async Task Run_JustAddOneWithCancellation_Unbatched_AsOrdered()
        {
            await Run_JustAddOneWithCancellation_With(maxBatchSize: 1, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_JustAddOneWithCancellation_Unbatched_OutOfOrder()
        {
            await Run_JustAddOneWithCancellation_With(maxBatchSize: 1, outOfOrder: true);
        }

        [Benchmark]
        public async Task Run_JustAddOneWithCancellation_MaxBatch_2_AsOrdered()
        {
            await Run_JustAddOneWithCancellation_With(maxBatchSize: 2, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_JustAddOneWithCancellation_MaxBatch_2_OutOfOrder()
        {
            await Run_JustAddOneWithCancellation_With(maxBatchSize: 2, outOfOrder: true);
        }

        [Benchmark]
        public async Task Run_JustAddOneWithCancellation_MaxBatch_4_AsOrdered()
        {
            await Run_JustAddOneWithCancellation_With(maxBatchSize: 4, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_JustAddOneWithCancellation_MaxBatch_4_OutOfOrder()
        {
            await Run_JustAddOneWithCancellation_With(maxBatchSize: 4, outOfOrder: true);
        }

        #endregion Benchmarks: JustAddOneWithCancellation

        #region Benchmarks: ReturnTaskCompletedTask

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTask_Unbatched_AsOrdered()
        {
            await Run_ReturnTaskCompletedTask_With(maxBatchSize: 1, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTask_Unbatched_OutOfOrder()
        {
            await Run_ReturnTaskCompletedTask_With(maxBatchSize: 1, outOfOrder: true);
        }

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTask_MaxBatch_2_AsOrdered()
        {
            await Run_ReturnTaskCompletedTask_With(maxBatchSize: 2, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTask_MaxBatch_2_OutOfOrder()
        {
            await Run_ReturnTaskCompletedTask_With(maxBatchSize: 2, outOfOrder: true);
        }

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTask_MaxBatch_4_AsOrdered()
        {
            await Run_ReturnTaskCompletedTask_With(maxBatchSize: 4, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTask_MaxBatch_4_OutOfOrder()
        {
            await Run_ReturnTaskCompletedTask_With(maxBatchSize: 4, outOfOrder: true);
        }

        #endregion Benchmarks: ReturnTaskCompletedTask

        #region Benchmarks: ReturnTaskCompletedTaskWithCancellation

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTaskWithCancellation_Unbatched_AsOrdered()
        {
            await Run_ReturnTaskCompletedTaskWithCancellation_With(maxBatchSize: 1, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTaskWithCancellation_Unbatched_OutOfOrder()
        {
            await Run_ReturnTaskCompletedTaskWithCancellation_With(maxBatchSize: 1, outOfOrder: true);
        }

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTaskWithCancellation_MaxBatch_2_AsOrdered()
        {
            await Run_ReturnTaskCompletedTaskWithCancellation_With(maxBatchSize: 2, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTaskWithCancellation_MaxBatch_2_OutOfOrder()
        {
            await Run_ReturnTaskCompletedTaskWithCancellation_With(maxBatchSize: 2, outOfOrder: true);
        }

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTaskWithCancellation_MaxBatch_4_AsOrdered()
        {
            await Run_ReturnTaskCompletedTaskWithCancellation_With(maxBatchSize: 4, outOfOrder: false);
        }

        [Benchmark]
        public async Task Run_ReturnTaskCompletedTaskWithCancellation_MaxBatch_4_OutOfOrder()
        {
            await Run_ReturnTaskCompletedTaskWithCancellation_With(maxBatchSize: 4, outOfOrder: true);
        }

        #endregion Benchmarks: ReturnTaskCompletedTaskWithCancellation

        #region Helpers

        private async Task Run_JustAddOne_With(int maxBatchSize, bool outOfOrder)
        {
            var batchResults = await ParallelAsync.ForEachAsync(InputNumbers, JustAddOne, maxBatchSize, outOfOrder, NumberOfItemsInCollection, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task Run_JustAddOneWithCancellation_With(int maxBatchSize, bool outOfOrder)
        {
            var batchResults = await ParallelAsync.ForEachAsync(InputNumbers, JustAddOneWithCancellation, maxBatchSize, outOfOrder, NumberOfItemsInCollection, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task Run_ReturnTaskCompletedTask_With(int maxBatchSize, bool outOfOrder)
        {
            await ParallelAsync.ForEachAsync(InputNumbers, ReturnTaskCompletedTask, maxBatchSize, outOfOrder, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task Run_ReturnTaskCompletedTaskWithCancellation_With(int maxBatchSize, bool outOfOrder)
        {
            await ParallelAsync.ForEachAsync(InputNumbers, ReturnTaskCompletedTaskWithCancellation, maxBatchSize, outOfOrder, CancellationToken.None).ConfigureAwait(false);
        }

        #endregion Helpers

        #region Delegates

        private static Task<int> JustAddOne(int number)
        {
            return Task.FromResult(number + 1);
        }

        private static Task<int> JustAddOneWithCancellation(int number, CancellationToken cancellationToken)
        {
            return Task.FromResult(number + 1);
        }

        private static Task ReturnTaskCompletedTask(int number)
        {
            return Task.CompletedTask;
        }

        private static Task ReturnTaskCompletedTaskWithCancellation(int number, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion Delegates

    }
}
