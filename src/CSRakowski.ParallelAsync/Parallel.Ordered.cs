﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.ParallelAsync
{
    public static partial class Parallel
    {
        private static async Task<IEnumerable<TResult>> ForEachAsyncImplOrdered<TResult, TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int batchSize, int estimatedResultSize, Func<TIn, Task<TResult>> func)
        {
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;

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

                    var batchResults = await Task.WhenAll(taskList).ConfigureAwait(false);
                    result.AddRange(batchResults);
                }
            }

            return result;
        }

        private static async Task<IEnumerable<TResult>> ForEachAsyncImplOrdered<TResult, TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int batchSize, int estimatedResultSize, Func<TIn, CancellationToken, Task<TResult>> func)
        {
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;

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

                    var batchResults = await Task.WhenAll(taskList).ConfigureAwait(false);
                    result.AddRange(batchResults);
                }
            }

            return result;
        }

        private static async Task ForEachAsyncImplOrdered<TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int batchSize, Func<TIn, Task> func)
        {
            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;

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

                    await Task.WhenAll(taskList).ConfigureAwait(false);
                }
            }
        }

        private static async Task ForEachAsyncImplOrdered<TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int batchSize, Func<TIn, CancellationToken, Task> func)
        {
            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;

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

                    await Task.WhenAll(taskList).ConfigureAwait(false);
                }
            }
        }
    }
}