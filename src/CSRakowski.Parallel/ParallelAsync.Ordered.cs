using CSRakowski.Parallel.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel
{
    public static partial class ParallelAsync
    {
        /// <summary>
        /// Implementation to run the specified async method for each item of the input collection in an batched manner, whilst preserving ordering as much as possible.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The collection of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="batchSize">The batch size to use</param>
        /// <param name="estimatedResultSize">The estimated size of the result collection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        private static Task<IEnumerable<TResult>> ForEachAsyncImplOrdered<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int batchSize, int estimatedResultSize, CancellationToken cancellationToken)
        {
            if (collection is TIn[] array)
            {
                return ForEachAsyncImplOrdered_Array<TResult, TIn>(array, func, batchSize, cancellationToken);
            }
            else
            {
                return ForEachAsyncImplOrdered_IEnumerable<TResult, TIn>(collection, func, batchSize, estimatedResultSize, cancellationToken);
            }
        }

        /// <summary>
        /// Default implementation to run the specified async method for each item of the input <see cref="IEnumerable{T}"/> in an batched manner, whilst preserving ordering as much as possible.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The <see cref="IEnumerable{T}"/> of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="batchSize">The batch size to use</param>
        /// <param name="estimatedResultSize">The estimated size of the result collection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        private static async Task<IEnumerable<TResult>> ForEachAsyncImplOrdered_IEnumerable<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int batchSize, int estimatedResultSize, CancellationToken cancellationToken)
        {
            // Using arrays is only marginally faster (in the best case, when the determined/estimated result size is correct)
            // In cases where the resultsize is off, we inch closer to the speed offered by List (for obvious reasons)
            // To prevent duplicating code, we are not going to be directly using arrays for the results collection here.
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, batchSize, false, estimatedResultSize);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                int batchId = 0;

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

                    ParallelAsyncEventSource.Log.BatchStart(batchId, taskList.Length);

                    var batchResults = await Task.WhenAll(taskList).ConfigureAwait(false);

                    result.AddRange(batchResults);

                    ParallelAsyncEventSource.Log.BatchStop(batchId);

                    batchId++;
                }
            }

            ParallelAsyncEventSource.Log.RunStop(runId);

            return result;
        }

        /// <summary>
        /// Special case implementation to run the specified async method for each item of the input <c>T[]</c> in an batched manner, whilst preserving ordering as much as possible.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The <c>T[]</c> of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="batchSize">The batch size to use</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        private static async Task<IEnumerable<TResult>> ForEachAsyncImplOrdered_Array<TResult, TIn>(TIn[] collection, Func<TIn, CancellationToken, Task<TResult>> func, int batchSize, CancellationToken cancellationToken)
        {
            var result = ListHelpers.GetList<TResult>(collection.Length);

            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, batchSize, false, collection.Length);

            int batchId = 0;
            int collectionIndex = 0;

            while (collectionIndex < collection.Length && !cancellationToken.IsCancellationRequested)
            {
                // TODO: Look into using Array.Copy for this?
                var taskList = new Task<TResult>[batchSize];

                int threadIndex;
                for (threadIndex = 0; threadIndex < batchSize && collectionIndex < collection.Length; threadIndex++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var element = collection[collectionIndex];

                    var task = func(element, cancellationToken);
                    taskList[threadIndex] = task;

                    collectionIndex++;
                }

                // If we reach the end, we need to ensure there are no NULLs in the taskList as Task.WhenAll breaks on those.
                if (threadIndex < batchSize)
                {
                    var temp = new Task<TResult>[threadIndex];
                    Array.Copy(taskList, temp, threadIndex);
                    taskList = temp;
                }

                ParallelAsyncEventSource.Log.BatchStart(batchId, taskList.Length);

                var batchResults = await Task.WhenAll(taskList).ConfigureAwait(false);

                result.AddRange(batchResults);

                ParallelAsyncEventSource.Log.BatchStop(batchId);

                batchId++;
            }

            ParallelAsyncEventSource.Log.RunStop(runId);

            return result;
        }

        /// <summary>
        /// Implementation to run the specified async method for each item of the input collection in an batched manner, whilst preserving ordering as much as possible.
        /// </summary>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The collection of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="batchSize">The batch size to use</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task"/> signaling completion</returns>
        private static Task ForEachAsyncImplOrdered<TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task> func, int batchSize, CancellationToken cancellationToken)
        {
            if (collection is TIn[] array)
            {
                return ForEachAsyncImplOrdered_Array<TIn>(array, func, batchSize, cancellationToken);
            }
            else
            {
                return ForEachAsyncImplOrdered_IEnumerable<TIn>(collection, func, batchSize, cancellationToken);
            }
        }

        /// <summary>
        /// Default implementation to run the specified async method for each item of the input <see cref="IEnumerable{T}"/> in an batched manner, whilst preserving ordering as much as possible.
        /// </summary>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The <see cref="IEnumerable{T}"/> of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="batchSize">The batch size to use</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task"/> signaling completion</returns>
        private static async Task ForEachAsyncImplOrdered_IEnumerable<TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task> func, int batchSize, CancellationToken cancellationToken)
        {
            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, batchSize, false, 0);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                int batchId = 0;

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

                    ParallelAsyncEventSource.Log.BatchStart(batchId, taskList.Length);

                    await Task.WhenAll(taskList).ConfigureAwait(false);

                    ParallelAsyncEventSource.Log.BatchStop(batchId);

                    batchId++;
                }
            }

            ParallelAsyncEventSource.Log.RunStop(runId);
        }

        /// <summary>
        /// Special case implementation to run the specified async method for each item of the input <c>T[]</c> in an batched manner, whilst preserving ordering as much as possible.
        /// </summary>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The <c>T[]</c> of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="batchSize">The batch size to use</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task"/> signaling completion</returns>
        private static async Task ForEachAsyncImplOrdered_Array<TIn>(TIn[] collection, Func<TIn, CancellationToken, Task> func, int batchSize, CancellationToken cancellationToken)
        {
            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, batchSize, false, collection.Length);

            int batchId = 0;
            int collectionIndex = 0;

            while (collectionIndex < collection.Length && !cancellationToken.IsCancellationRequested)
            {
                var taskList = new Task[batchSize];

                int threadIndex;
                for (threadIndex = 0; threadIndex < batchSize && collectionIndex < collection.Length; threadIndex++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var element = collection[collectionIndex];

                    var task = func(element, cancellationToken);
                    taskList[threadIndex] = task;

                    collectionIndex++;
                }

                // If we reach the end, we need to ensure there are no NULLs in the taskList as Task.WhenAll breaks on those.
                if (threadIndex < batchSize)
                {
                    var temp = new Task[threadIndex];
                    Array.Copy(taskList, temp, threadIndex);
                    taskList = temp;
                }

                ParallelAsyncEventSource.Log.BatchStart(batchId, taskList.Length);

                await Task.WhenAll(taskList).ConfigureAwait(false);

                ParallelAsyncEventSource.Log.BatchStop(batchId);

                batchId++;
            }

            ParallelAsyncEventSource.Log.RunStop(runId);
        }
    }
}
