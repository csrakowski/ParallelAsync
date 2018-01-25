using CSRakowski.Parallel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel
{
    public static partial class ParallelAsync
    {
        #region IEnumerable<T>

        /// <summary>
        /// Implementation to run the specified async method for each item of the input collection in an batched manner, allowing out of order processing.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The collection of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="batchSize">The batch size to use</param>
        /// <param name="estimatedResultSize">The estimated size of the result collection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        private static async Task<IEnumerable<TResult>> ForEachAsyncImplUnordered<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int batchSize, int estimatedResultSize, CancellationToken cancellationToken)
        {
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, batchSize, true, estimatedResultSize);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                int batchId = 0;
                var taskList = ListHelpers.GetList<Task<TResult>>(batchSize);

                while (!cancellationToken.IsCancellationRequested)
                {
                    // check the hasNext from the previous run, if false; don't call MoveNext() again
                    // call MoveNext() and assign it to the hasNext variable, then check if this run still had a next
                    if (hasNext && (hasNext = enumerator.MoveNext()))
                    {
                        var element = enumerator.Current;

                        var task = func(element, cancellationToken);
                        taskList.Add(task);

                        if (taskList.Count < batchSize)
                        {
                            continue;
                        }
                    }

                    if (!hasNext && taskList.Count == 0)
                    {
                        break;
                    }

                    ParallelAsyncEventSource.Log.BatchStart(runId, batchId, taskList.Count);

                    await Task.WhenAny(taskList).ConfigureAwait(false);

                    var completed = taskList.FindAll(t => t.IsCompleted);
                    foreach (var t in completed)
                    {
                        result.Add(t.Result);
                        taskList.Remove(t);
                    }

                    ParallelAsyncEventSource.Log.BatchStop(runId, batchId);

                    batchId++;
                }
            }

            ParallelAsyncEventSource.Log.RunStop(runId);

            return result;
        }

        /// <summary>
        /// Implementation to run the specified async method for each item of the input collection in an batched manner, allowing out of order processing.
        /// </summary>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The collection of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="batchSize">The batch size to use</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task"/> signaling completion</returns>
        private static async Task ForEachAsyncImplUnordered<TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task> func, int batchSize, CancellationToken cancellationToken)
        {
            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, batchSize, true, 0);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                int batchId = 0;
                var taskList = ListHelpers.GetList<Task>(batchSize);

                while (!cancellationToken.IsCancellationRequested)
                {
                    // check the hasNext from the previous run, if false; don't call MoveNext() again
                    // call MoveNext() and assign it to the hasNext variable, then check if this run still had a next
                    if (hasNext && (hasNext = enumerator.MoveNext()))
                    {
                        var element = enumerator.Current;

                        var task = func(element, cancellationToken);
                        taskList.Add(task);

                        if (taskList.Count < batchSize)
                        {
                            continue;
                        }
                    }

                    if (!hasNext && taskList.Count == 0)
                    {
                        break;
                    }

                    ParallelAsyncEventSource.Log.BatchStart(runId, batchId, taskList.Count);

                    await Task.WhenAny(taskList).ConfigureAwait(false);

                    taskList.RemoveAll(t => t.IsCompleted);

                    ParallelAsyncEventSource.Log.BatchStop(runId, batchId);

                    batchId++;
                }
            }

            ParallelAsyncEventSource.Log.RunStop(runId);
        }


        #endregion IEnumerable<T>

        #region IAsyncEnumerable<T>

        private static async Task<IEnumerable<TResult>> ForEachAsyncImplUnordered<TResult, TIn>(IAsyncEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int batchSize, int estimatedResultSize, CancellationToken cancellationToken)
        {
            var result = ListHelpers.GetList<TResult>(estimatedResultSize);

            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, batchSize, true, estimatedResultSize);

            var enumerator = collection.GetAsyncEnumerator();
            try
            {
                var hasNext = true;
                long batchId = 0;
                var taskList = ListHelpers.GetList<Task<TResult>>(batchSize);

                while (!cancellationToken.IsCancellationRequested)
                {
                    // check the hasNext from the previous run, if false; don't call MoveNext() again
                    // call MoveNext() and assign it to the hasNext variable, then check if this run still had a next
                    if (hasNext && (hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false)))
                    {
                        var element = enumerator.Current;

                        var task = func(element, cancellationToken);
                        taskList.Add(task);

                        if (taskList.Count < batchSize)
                        {
                            continue;
                        }
                    }

                    if (!hasNext && taskList.Count == 0)
                    {
                        break;
                    }

                    ParallelAsyncEventSource.Log.BatchStart(batchId, taskList.Count);

                    await Task.WhenAny(taskList).ConfigureAwait(false);

                    var completed = taskList.Where(t => t.IsCompleted).ToList();
                    foreach (var t in completed)
                    {
                        result.Add(t.Result);
                        taskList.Remove(t);
                    }

                    ParallelAsyncEventSource.Log.BatchStop(batchId);

                    batchId++;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            ParallelAsyncEventSource.Log.RunStop(runId);

            return result;
        }

        private static async Task ForEachAsyncImplUnordered<TIn>(IAsyncEnumerable<TIn> collection, Func<TIn, CancellationToken, Task> func, int batchSize, CancellationToken cancellationToken)
        {
            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, batchSize, true, 0);

            var enumerator = collection.GetAsyncEnumerator();
            try
            {
                var hasNext = true;
                long batchId = 0;
                var taskList = ListHelpers.GetList<Task>(batchSize);

                while (!cancellationToken.IsCancellationRequested)
                {
                    // check the hasNext from the previous run, if false; don't call MoveNext() again
                    // call MoveNext() and assign it to the hasNext variable, then check if this run still had a next
                    if (hasNext && (hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false)))
                    {
                        var element = enumerator.Current;

                        var task = func(element, cancellationToken);
                        taskList.Add(task);

                        if (taskList.Count < batchSize)
                        {
                            continue;
                        }
                    }

                    if (!hasNext && taskList.Count == 0)
                    {
                        break;
                    }

                    ParallelAsyncEventSource.Log.BatchStart(batchId, taskList.Count);

                    await Task.WhenAny(taskList).ConfigureAwait(false);

                    var completed = taskList.Where(t => t.IsCompleted).ToList();
                    foreach (var t in completed)
                    {
                        taskList.Remove(t);
                    }

                    ParallelAsyncEventSource.Log.BatchStop(batchId);

                    batchId++;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            ParallelAsyncEventSource.Log.RunStop(runId);
        }

        #endregion IAsyncEnumerable<T>
    }
}
