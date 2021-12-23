using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSRakowski.Parallel;

namespace Profiling
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            const int numberOfElements = 1000000;
            const int batchSize = 64;
            const bool outOfOrder = false;
            var input = Enumerable.Range(1, numberOfElements).ToList();

            //await ParallelAsync.ForEachAsync(
            var results = await ParallelAsync.ForEachAsync(
                                collection: input,
                                func: AddOne,
                                maxBatchSize: batchSize,
                                allowOutOfOrderProcessing: outOfOrder,
                                estimatedResultSize: numberOfElements,
                                cancellationToken: CancellationToken.None);

            /*/
            return 1;
            /*/
            var resultCount = results.Count();
            return resultCount;
            //*/
        }

        private static Task<int> AddOne(int input)
        {
            return Task.FromResult(1 + input);
        }

        private static Task CompletedTask(int input)
        {
            return Task.CompletedTask;
        }
    }
}
