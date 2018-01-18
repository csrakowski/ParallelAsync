using CSRakowski.Parallel.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel
{
    public static partial class ParallelAsync
    {
        private static async Task<IEnumerable<TResult>> ForEachAsyncImplOrdered<TResult, TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int batchSize, int estimatedResultSize, Func<TIn, Task<TResult>> func)
        {
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            long runId = EventSource.GetRunId();
            EventSource.RunStart(runId, batchSize, false, estimatedResultSize);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                long batchId = 0;

                while (hasNext && !cancellationToken.IsCancellationRequested)
                {
                    var taskList = new Task<TResult>[batchSize];

                    int threadIndex;
                    for (threadIndex = 0; threadIndex < batchSize; threadIndex++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        hasNext = enumerator.MoveNext();

                        if (!hasNext)
                        {
                            break;
                        }

                        var element = enumerator.Current;

                        var task = func(element);
                        taskList[threadIndex] = task;
                    }

                    // If we reach the end, we need to ensure there are no NULLs in the taskList as Task.WhenAll breaks on those.
                    if (threadIndex < batchSize)
                    {
                        var temp = new Task<TResult>[threadIndex];
                        Array.Copy(taskList, temp, threadIndex);
                        taskList = temp;
                    }

                    EventSource.BatchStart(batchId, taskList.Length);

                    var batchResults = await Task.WhenAll(taskList).ConfigureAwait(false);
                    result.AddRange(batchResults);

                    EventSource.BatchStop(batchId);

                    batchId++;
                }
            }

            EventSource.RunStop(runId);

            return result;
        }

        private static async Task<IEnumerable<TResult>> ForEachAsyncImplOrdered<TResult, TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int batchSize, int estimatedResultSize, Func<TIn, CancellationToken, Task<TResult>> func)
        {
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            long runId = EventSource.GetRunId();
            EventSource.RunStart(runId, batchSize, false, estimatedResultSize);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                long batchId = 0;

                while (hasNext && !cancellationToken.IsCancellationRequested)
                {
                    var taskList = new Task<TResult>[batchSize];

                    int threadIndex;
                    for (threadIndex = 0; threadIndex < batchSize; threadIndex++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        hasNext = enumerator.MoveNext();

                        if (!hasNext)
                        {
                            break;
                        }

                        var element = enumerator.Current;

                        var task = func(element, cancellationToken);
                        taskList[threadIndex] = task;
                    }

                    // If we reach the end, we need to ensure there are no NULLs in the taskList as Task.WhenAll breaks on those.
                    if (threadIndex < batchSize)
                    {
                        var temp = new Task<TResult>[threadIndex];
                        Array.Copy(taskList, temp, threadIndex);
                        taskList = temp;
                    }

                    EventSource.BatchStart(batchId, taskList.Length);

                    var batchResults = await Task.WhenAll(taskList).ConfigureAwait(false);
                    result.AddRange(batchResults);

                    EventSource.BatchStop(batchId);

                    batchId++;
                }
            }

            EventSource.RunStop(runId);

            return result;
        }

        private static async Task ForEachAsyncImplOrdered<TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int batchSize, Func<TIn, Task> func)
        {
            long runId = EventSource.GetRunId();
            EventSource.RunStart(runId, batchSize, false, 0);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                long batchId = 0;

                while (hasNext && !cancellationToken.IsCancellationRequested)
                {
                    var taskList = new Task[batchSize];

                    int threadIndex;
                    for (threadIndex = 0; threadIndex < batchSize; threadIndex++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        hasNext = enumerator.MoveNext();

                        if (!hasNext)
                        {
                            break;
                        }

                        var element = enumerator.Current;

                        var task = func(element);
                        taskList[threadIndex] = task;
                    }

                    // If we reach the end, we need to ensure there are no NULLs in the taskList as Task.WhenAll breaks on those.
                    if (threadIndex < batchSize)
                    {
                        var temp = new Task[threadIndex];
                        Array.Copy(taskList, temp, threadIndex);
                        taskList = temp;
                    }

                    EventSource.BatchStart(batchId, taskList.Length);

                    await Task.WhenAll(taskList).ConfigureAwait(false);

                    EventSource.BatchStop(batchId);

                    batchId++;
                }
            }

            EventSource.RunStop(runId);
        }

        private static async Task ForEachAsyncImplOrdered<TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int batchSize, Func<TIn, CancellationToken, Task> func)
        {
            long runId = EventSource.GetRunId();
            EventSource.RunStart(runId, batchSize, false, 0);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                long batchId = 0;

                while (hasNext && !cancellationToken.IsCancellationRequested)
                {
                    var taskList = new Task[batchSize];

                    int threadIndex;
                    for (threadIndex = 0; threadIndex < batchSize; threadIndex++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        hasNext = enumerator.MoveNext();

                        if (!hasNext)
                        {
                            break;
                        }

                        var element = enumerator.Current;

                        var task = func(element, cancellationToken);
                        taskList[threadIndex] = task;
                    }

                    // If we reach the end, we need to ensure there are no NULLs in the taskList as Task.WhenAll breaks on those.
                    if (threadIndex < batchSize)
                    {
                        var temp = new Task[threadIndex];
                        Array.Copy(taskList, temp, threadIndex);
                        taskList = temp;
                    }

                    EventSource.BatchStart(batchId, taskList.Length);

                    await Task.WhenAll(taskList).ConfigureAwait(false);

                    EventSource.BatchStop(batchId);

                    batchId++;
                }
            }

            EventSource.RunStop(runId);
        }
    }
}
