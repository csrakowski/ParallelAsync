
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
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace CSRakowski.Parallel.Benchmarks
{
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [SimpleJob(RuntimeMoniker.Net48, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31, baseline: false)]
    [SimpleJob(RuntimeMoniker.Net50, baseline: false)]
    [SimpleJob(RuntimeMoniker.Net60, baseline: false)]
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
            return ParallelAsync.ForEachAsync(InputNumbersList, TestFunctions.JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_ReadOnlyList()
        {
            return ParallelAsync.ForEachAsync(InputNumbersReadOnlyList, TestFunctions.JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_Enumerable()
        {
            return ParallelAsync.ForEachAsync(InputNumbersEnumerable, TestFunctions.JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_Array()
        {
            return ParallelAsync.ForEachAsync(InputNumbersArray, TestFunctions.JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }


        // ReturnTaskCompletedTask

        [Benchmark(Baseline = true), BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_List()
        {
            return ParallelAsync.ForEachAsync(InputNumbersList, TestFunctions.ReturnCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_ReadOnlyList()
        {
            return ParallelAsync.ForEachAsync(InputNumbersReadOnlyList, TestFunctions.ReturnCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_Enumerable()
        {
            return ParallelAsync.ForEachAsync(InputNumbersEnumerable, TestFunctions.ReturnCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_Array()
        {
            return ParallelAsync.ForEachAsync(InputNumbersArray, TestFunctions.ReturnCompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }
    }
}
