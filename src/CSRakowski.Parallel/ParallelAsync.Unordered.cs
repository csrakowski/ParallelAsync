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
        //TODO: #37 - Look into using Arrays directly, instead of relying on Adding/Removing using List (Perf/GC overhead)

        private static async Task<IEnumerable<TResult>> ForEachAsyncImplUnordered<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int batchSize, int estimatedResultSize, CancellationToken cancellationToken)
        {
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, batchSize, true, estimatedResultSize);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                long batchId = 0;
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

            ParallelAsyncEventSource.Log.RunStop(runId);

            return result;
        }

        private static async Task ForEachAsyncImplUnordered<TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task> func, int batchSize, CancellationToken cancellationToken)
        {
            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, batchSize, true, 0);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;
                long batchId = 0;
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

            ParallelAsyncEventSource.Log.RunStop(runId);
        }
    }
}
