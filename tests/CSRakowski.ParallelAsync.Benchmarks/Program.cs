using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Benchmarks
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // TODO: Rewrite to Benchmark.NET
            await Performance_Degradation_Run(0, 10000, true);
            await Performance_Degradation_Run(0, 10000, false);
            await Performance_Degradation_Run(1, 10000, true);
            await Performance_Degradation_Run(1, 10000, false);
            await Performance_Degradation_Run(2, 10000, true);
            await Performance_Degradation_Run(2, 10000, false);
            await Performance_Degradation_Run(8, 10000, true);
            await Performance_Degradation_Run(8, 10000, false);
            await Performance_Degradation_Run(128, 10000, true);
            await Performance_Degradation_Run(128, 10000, false);
        }

        private static async Task Performance_Degradation_Run(int maxBatchSize, int numberOfItemsInCollection, bool outOfOrder)
        {
            const int numberOfRuns = 5;

            var numbers = Enumerable.Range(0, numberOfItemsInCollection).ToList();

            #region Pre-benchmark JIT run

            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("Single runs to pre-jit it all");

            await ParallelAsync.ForEachAsync(numbers, JustAddOne, maxBatchSize, outOfOrder, numberOfItemsInCollection, CancellationToken.None).ConfigureAwait(false);

            Console.ForegroundColor = defaultColor;

            Console.WriteLine("Pre-jit done!\nRunning.");

            #endregion Pre-benchmark JIT run

            var results = new List<TestResult>(numberOfRuns + 1);

            for (int runCount = 0; runCount < numberOfRuns; runCount++)
            {
                var timer = Stopwatch.StartNew();

                var batchResults = await ParallelAsync.ForEachAsync(numbers, JustAddOne, maxBatchSize, outOfOrder, numberOfItemsInCollection, CancellationToken.None).ConfigureAwait(false);

                timer.Stop();
                var resultCount = batchResults.Count();
                var elapsed = timer.Elapsed.TotalMilliseconds;

                var testResult = new TestResult(runCount, numberOfItemsInCollection, resultCount, elapsed);
                results.Add(testResult);
            }

            var totalElapsed = results.Sum(r => r.ElapsedMilliseconds);
            var totalResults = results.Sum(r => r.NumberOfResults);
            var average = new TestResult(-1, numberOfItemsInCollection, totalResults, totalElapsed);
            results.Add(average);

            var sb = new StringBuilder($"Results {nameof(ParallelAsync)} ({nameof(maxBatchSize)}:{maxBatchSize}, {nameof(numberOfItemsInCollection)}:{numberOfItemsInCollection}, {nameof(outOfOrder)}:{outOfOrder})")
                            .Append("\n\nRun;NumberOfMessages;Elapsed (ms);Throughput (messages/ms);\n");

            foreach (var result in results)
            {
                sb.Append(result.RunID).Append(";")
                    .Append(result.NumberOfResults).Append(";")
                    .Append(result.ElapsedMilliseconds).Append(";")
                    .Append(result.MessagesPerMillisecond).Append(";\n");
            }

            var resultCsv = sb.ToString();
            Console.WriteLine(resultCsv);
        }

        private static Task<int> JustAddOne(int number)
        {
            return Task.FromResult(number + 1);
        }
    }
}
