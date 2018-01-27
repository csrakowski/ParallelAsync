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
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace CSRakowski.Parallel.Benchmarks
{
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory, BenchmarkLogicalGroupRule.ByParams)]
    [ClrJob(isBaseline: true)]
    //[CoreJob(isBaseline: false)]
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
            return ParallelAsync.ForEachAsync(InputNumbers, Func_JustAddOne, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("JustAddOne")]
        public Task JustAddOne_NotWrapped()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, Func_JustAddOne_CT, MaxBatchSize, AllowOutOfOrder, NumberOfItemsInCollection, CancellationToken.None);
        }


        [Benchmark, BenchmarkCategory("CompletedTask")]
        public Task CompletedTask_Wrapped()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, Func_CompletedTask, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("CompletedTask")]
        public Task CompletedTask_NotWrapped()
        {
            return ParallelAsync.ForEachAsync(InputNumbers, Func_CompletedTask_CT, MaxBatchSize, AllowOutOfOrder, CancellationToken.None);
        }

        #region Delegates

        private static Task<int> Func_JustAddOne(int number)
        {
            return Task.FromResult(number + 1);
        }

        private static Task<int> Func_JustAddOne_CT(int number, CancellationToken cancellationToken)
        {
            return Task.FromResult(number + 1);
        }

        private static Task Func_CompletedTask(int number)
        {
            return Task.CompletedTask;
        }

        private static Task Func_CompletedTask_CT(int number, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion Delegates
    }
}
