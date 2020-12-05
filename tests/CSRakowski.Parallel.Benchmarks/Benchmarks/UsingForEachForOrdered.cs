
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
    [SimpleJob(RuntimeMoniker.NetCoreApp50, baseline: false)]
    public class UsingForEachForOrdered
    {
        private const int NumberOfItemsInCollection = 100000;

        private readonly int[] InputNumbersArray;
        private readonly List<int> InputNumbersList;
        private readonly ReadOnlyCollection<int> InputNumbersReadOnlyList;
        private IEnumerable<int> InputNumbersEnumerable { get { return Enumerable.Range(0, NumberOfItemsInCollection); } }

        public UsingForEachForOrdered()
        {
            InputNumbersArray = Enumerable.Range(0, NumberOfItemsInCollection).ToArray();
            InputNumbersList = InputNumbersArray.ToList();
            InputNumbersReadOnlyList = InputNumbersList.AsReadOnly();
        }

        public int MaxBatchSize { get; set; } = 8;

        public bool AllowOutOfOrder { get; set; } = false;

        // JustAddOne

        [Benchmark(Baseline = true), BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_List()
        {
            return ParallelAsync.ForEachAsync(InputNumbersList, TestFunctions.JustAddOne_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_ReadOnlyList()
        {
            return ParallelAsync.ForEachAsync(InputNumbersReadOnlyList, TestFunctions.JustAddOne_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_Enumerable()
        {
            return ParallelAsync.ForEachAsync(InputNumbersEnumerable, TestFunctions.JustAddOne_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_Array()
        {
            return ParallelAsync.ForEachAsync(InputNumbersArray, TestFunctions.JustAddOne_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        // ReturnTaskCompletedTask

        [Benchmark(Baseline = true), BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_List()
        {
            return ParallelAsync.ForEachAsync(InputNumbersList, TestFunctions.ReturnCompletedTask_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_ReadOnlyList()
        {
            return ParallelAsync.ForEachAsync(InputNumbersReadOnlyList, TestFunctions.ReturnCompletedTask_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_Enumerable()
        {
            return ParallelAsync.ForEachAsync(InputNumbersEnumerable, TestFunctions.ReturnCompletedTask_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark, BenchmarkCategory("ReturnTaskCompletedTask")]
        public Task ReturnTaskCompletedTask_Array()
        {
            return ParallelAsync.ForEachAsync(InputNumbersArray, TestFunctions.ReturnCompletedTask_WithCancellationToken, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }
    }
}
