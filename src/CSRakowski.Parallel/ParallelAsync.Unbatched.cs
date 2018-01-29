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
        /// Implementation to run the specified async method for each item of the input collection in an unbatched manner.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The collection of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="estimatedResultSize">The estimated size of the result collection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        private static Task<IEnumerable<TResult>> ForEachAsyncImplUnbatched<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int estimatedResultSize, CancellationToken cancellationToken)
        {
            if (collection is TIn[] array)
            {
                return ForEachAsyncImplUnbatched_Array<TResult, TIn>(array, func, cancellationToken);
            }
            else if (collection is IList<TIn> list)
            {
                return ForEachAsyncImplUnbatched_IList<TResult, TIn>(list, func, cancellationToken);
            }
            else
            {
                return ForEachAsyncImplUnbatched_IEnumerable<TResult, TIn>(collection, func, estimatedResultSize, cancellationToken);
            }
        }

        /// <summary>
        /// Default implementation to run the specified async method for each item of the input <see cref="IEnumerable{T}"/> in an unbatched manner.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The <see cref="IEnumerable{T}"/> of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="estimatedResultSize">The estimated size of the result collection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        private static async Task<IEnumerable<TResult>> ForEachAsyncImplUnbatched_IEnumerable<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int estimatedResultSize, CancellationToken cancellationToken)
        {
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, 1, false, estimatedResultSize);

            int batchId = 0;
            foreach (var element in collection)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ParallelAsyncEventSource.Log.BatchStart(batchId, 1);

                var resultElement = await func(element, cancellationToken).ConfigureAwait(false);
                result.Add(resultElement);

                ParallelAsyncEventSource.Log.BatchStop(batchId);

                batchId++;
            }

            ParallelAsyncEventSource.Log.RunStop(runId);

            return result;
        }

        /// <summary>
        /// Special case implementation to run the specified async method for each item of the input <see cref="IList{T}"/> in an unbatched manner.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The <see cref="IList{T}"/> of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        private static async Task<IEnumerable<TResult>> ForEachAsyncImplUnbatched_IList<TResult, TIn>(IList<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken)
        {
            var collectionCount = collection.Count;

            var result = ListHelpers.GetList<TResult>(collectionCount);

            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, 1, false, 0);

            for (int batchId = 0; batchId < collectionCount; batchId++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ParallelAsyncEventSource.Log.BatchStart(batchId, 1);

                var element = collection[batchId];
                var resultElement = await func(element, cancellationToken).ConfigureAwait(false);
                result.Add(resultElement);

                ParallelAsyncEventSource.Log.BatchStop(batchId);
            }

            ParallelAsyncEventSource.Log.RunStop(runId);

            return result;
        }

        /// <summary>
        /// Special case implementation to run the specified async method for each item of the input <c>T[]</c> in an unbatched manner.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The <c>T[]</c> of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        private static async Task<IEnumerable<TResult>> ForEachAsyncImplUnbatched_Array<TResult, TIn>(TIn[] collection, Func<TIn, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken)
        {
            var result = ListHelpers.GetList<TResult>(collection.Length);

            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, 1, false, 0);

            for (int batchId = 0; batchId < collection.Length; batchId++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ParallelAsyncEventSource.Log.BatchStart(batchId, 1);

                var element = collection[batchId];
                var resultElement = await func(element, cancellationToken).ConfigureAwait(false);
                result.Add(resultElement);

                ParallelAsyncEventSource.Log.BatchStop(batchId);
            }

            ParallelAsyncEventSource.Log.RunStop(runId);

            return result;
        }

        /// <summary>
        /// Implementation to run the specified async method for each item of the input collection in an unbatched manner.
        /// </summary>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The collection of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task"/> signaling completion</returns>
        private static Task ForEachAsyncImplUnbatched<TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task> func, CancellationToken cancellationToken)
        {
            if (collection is TIn[] array)
            {
                return ForEachAsyncImplUnbatched_Array<TIn>(array, func, cancellationToken);
            }
            else if (collection is IList<TIn> list)
            {
                return ForEachAsyncImplUnbatched_IList<TIn>(list, func, cancellationToken);
            }
            else
            {
                return ForEachAsyncImplUnbatched_IEnumerable<TIn>(collection, func, cancellationToken);
            }
        }

        /// <summary>
        /// Default implementation to run the specified async method for each item of the input <see cref="IEnumerable{T}"/> in an unbatched manner.
        /// </summary>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The <see cref="IEnumerable{T}"/> of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task"/> signaling completion</returns>
        private static async Task ForEachAsyncImplUnbatched_IEnumerable<TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task> func, CancellationToken cancellationToken)
        {
            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, 1, false, 0);

            int batchId = 0;
            foreach (var element in collection)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ParallelAsyncEventSource.Log.BatchStart(batchId, 1);

                await func(element, cancellationToken).ConfigureAwait(false);

                ParallelAsyncEventSource.Log.BatchStop(batchId);

                batchId++;
            }

            ParallelAsyncEventSource.Log.RunStop(runId);
        }

        /// <summary>
        /// Special case implementation to run the specified async method for each item of the input <see cref="IList{T}"/> in an unbatched manner.
        /// </summary>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The <see cref="IList{T}"/> of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task"/> signaling completion</returns>
        private static async Task ForEachAsyncImplUnbatched_IList<TIn>(IList<TIn> collection, Func<TIn, CancellationToken, Task> func, CancellationToken cancellationToken)
        {
            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, 1, false, 0);

            var collectionCount = collection.Count;
            for (int batchId = 0; batchId < collectionCount; batchId++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ParallelAsyncEventSource.Log.BatchStart(batchId, 1);

                var element = collection[batchId];
                await func(element, cancellationToken).ConfigureAwait(false);

                ParallelAsyncEventSource.Log.BatchStop(batchId);
            }

            ParallelAsyncEventSource.Log.RunStop(runId);
        }

        /// <summary>
        /// Special case implementation to run the specified async method for each item of the input <c>T[]</c> in an unbatched manner.
        /// </summary>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The <c>T[]</c> of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task"/> signaling completion</returns>
        private static async Task ForEachAsyncImplUnbatched_Array<TIn>(TIn[] collection, Func<TIn, CancellationToken, Task> func, CancellationToken cancellationToken)
        {
            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, 1, false, 0);

            for (int batchId = 0; batchId < collection.Length; batchId++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ParallelAsyncEventSource.Log.BatchStart(batchId, 1);

                var element = collection[batchId];
                await func(element, cancellationToken).ConfigureAwait(false);

                ParallelAsyncEventSource.Log.BatchStop(batchId);
            }

            ParallelAsyncEventSource.Log.RunStop(runId);
        }
    }
}
