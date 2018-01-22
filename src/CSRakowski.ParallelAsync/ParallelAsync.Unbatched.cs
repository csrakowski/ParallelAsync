using CSRakowski.Parallel.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel
{
    public static partial class ParallelAsync
    {
        private static async Task<IEnumerable<TResult>> ForEachAsyncImplUnbatched<TResult, TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int estimatedResultSize, Func<TIn, CancellationToken, Task<TResult>> func)
        {
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            long runId = EventSource.GetRunId();
            EventSource.RunStart(runId, 1, false, estimatedResultSize);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                long batchId = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    hasNext = enumerator.MoveNext();

                    if (!hasNext)
                    {
                        break;
                    }

                    EventSource.BatchStart(batchId, 1);

                    var element = enumerator.Current;
                    var resultElement = await func(element, cancellationToken).ConfigureAwait(false);
                    result.Add(resultElement);

                    EventSource.BatchStop(batchId);

                    batchId++;
                }
            }

            EventSource.RunStop(runId);

            return result;
        }

        private static async Task ForEachAsyncImplUnbatched<TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, Func<TIn, CancellationToken, Task> func)
        {
            long runId = EventSource.GetRunId();
            EventSource.RunStart(runId, 1, false, 0);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                long batchId = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    hasNext = enumerator.MoveNext();

                    if (!hasNext)
                    {
                        break;
                    }

                    EventSource.BatchStart(batchId, 1);

                    var element = enumerator.Current;
                    await func(element, cancellationToken).ConfigureAwait(false);

                    EventSource.BatchStop(batchId);

                    batchId++;
                }
            }

            EventSource.RunStop(runId);
        }
    }
}
