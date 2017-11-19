using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSRakowski.ParallelAsync;
using System.Diagnostics;

namespace CSRakowski.ParallelAsync.Tests
{
    [TestFixture, Category("ParallelAsync Performance Degradation Tests")]
    public class ParallelAsyncBenchmarks
    {
        private class TestResult
        {
            public TestResult(int runId, int numberOfJobs, int numberOfResults, double elapsedMilliseconds)
            {
                RunID = (runId == -1) ? "Average" : runId.ToString();
                NumberOfJobs = numberOfJobs;
                NumberOfResults = numberOfResults;
                ElapsedMilliseconds = elapsedMilliseconds;
            }

            public string RunID { get; }

            public int NumberOfJobs { get; }

            public int NumberOfResults { get; }

            public double ElapsedMilliseconds { get; }

            public double MessagesPerMillisecond => NumberOfResults / ElapsedMilliseconds;
        }

        [TestCase(0, 10000, true)]
        [TestCase(0, 10000, false)]
        [TestCase(1, 10000, true)]
        [TestCase(1, 10000, false)]
        [TestCase(2, 10000, true)]
        [TestCase(2, 10000, false)]
        [TestCase(8, 10000, true)]
        [TestCase(8, 10000, false)]
        [TestCase(128, 10000, true)]
        [TestCase(128, 10000, false)]
        [TestCase(0, 1000000, true)]
        [TestCase(0, 1000000, false)]
        [TestCase(1, 1000000, true)]
        [TestCase(1, 1000000, false)]
        [TestCase(8, 1000000, true)]
        [TestCase(8, 1000000, false)]
        public async Task Performance_Degradation_Run(int maxBatchSize, int numberOfItemsInCollection, bool outOfOrder)
        {
            const int numberOfRuns = 5;

            var numbers = Enumerable.Range(0, numberOfItemsInCollection).ToList();

            #region Pre-benchmark JIT run

            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("Single runs to pre-jit it all");

            await Parallel.ForEachAsync(numbers, JustAddOne, maxBatchSize, outOfOrder, numberOfItemsInCollection, CancellationToken.None).ConfigureAwait(false);

            Console.ForegroundColor = defaultColor;

            Console.WriteLine("Pre-jit done!\nRunning.");

            #endregion Pre-benchmark JIT run

            var results = new List<TestResult>(numberOfRuns + 1);

            for (int runCount = 0; runCount < numberOfRuns; runCount++)
            {
                var timer = Stopwatch.StartNew();

                var batchResults = await Parallel.ForEachAsync(numbers, JustAddOne, maxBatchSize, outOfOrder, numberOfItemsInCollection, CancellationToken.None).ConfigureAwait(false);

                timer.Stop();
                var resultCount = batchResults.ToList().Count;
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
