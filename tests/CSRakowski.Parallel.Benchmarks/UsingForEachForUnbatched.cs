
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace CSRakowski.Parallel.Benchmarks
{
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [ClrJob]
    public class UsingForEachForUnbatched
    {
        private const int NumberOfItemsInCollection = 1000000;

        private readonly int[] InputNumbersArray;
        private readonly List<int> InputNumbersList;
        private readonly ReadOnlyCollection<int> InputNumbersReadOnlyList;
        private IEnumerable<int> InputNumbersEnumerable { get { return Enumerable.Range(0, NumberOfItemsInCollection); } }

        public UsingForEachForUnbatched()
        {
            InputNumbersArray = Enumerable.Range(0, NumberOfItemsInCollection).ToArray();
            InputNumbersList = InputNumbersArray.ToList();
            InputNumbersReadOnlyList = InputNumbersList.AsReadOnly();
        }

        public int MaxBatchSize { get; set; } = 1;

        public bool AllowOutOfOrder { get; set; } = false;


        // JustAddOne

        [Benchmark(Baseline = true), BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_List()
        {
            return ParallelAsync.ForEachAsync(InputNumbersList, Func_JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_ReadOnlyList()
        {
            return ParallelAsync.ForEachAsync(InputNumbersReadOnlyList, Func_JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_Enumerable()
        {
            return ParallelAsync.ForEachAsync(InputNumbersEnumerable, Func_JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_Array()
        {
            return ParallelAsync.ForEachAsync(InputNumbersArray, Func_JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }


        // ReturnTaskCompletedTask

        [Benchmark(Baseline = true), BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_List()
        {
            return ParallelAsync.ForEachAsync(InputNumbersList, Func_ReturnTaskCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_ReadOnlyList()
        {
            return ParallelAsync.ForEachAsync(InputNumbersReadOnlyList, Func_ReturnTaskCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_Enumerable()
        {
            return ParallelAsync.ForEachAsync(InputNumbersEnumerable, Func_ReturnTaskCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_Array()
        {
            return ParallelAsync.ForEachAsync(InputNumbersArray, Func_ReturnTaskCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        #region Delegates

        private static Task<int> Func_JustAddOne(int number)
        {
            return Task.FromResult(number + 1);
        }

        private static Task Func_ReturnTaskCompletedTask(int number)
        {
            return Task.CompletedTask;
        }

        #endregion Delegates
    }
}
