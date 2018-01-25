using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSRakowski.Parallel;

namespace Profiling
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var numberOfElements = 1000000;
            var input = Enumerable.Range(1, numberOfElements).ToList();

            var results = await ParallelAsync.ForEachAsync(
                                collection: input,
                                func: AddOne,
                                maxBatchSize: 4,
                                allowOutOfOrderProcessing: false,
                                estimatedResultSize: numberOfElements,
                                cancellationToken: CancellationToken.None);

            var resultCount = results.Count();

            return resultCount;
        }

        static Task<int> AddOne(int input)
        {
            return Task.FromResult(1 + input);
        }
    }
}
