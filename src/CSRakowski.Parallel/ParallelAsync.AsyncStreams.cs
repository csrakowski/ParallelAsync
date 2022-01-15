using CSRakowski.Parallel.Helpers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel
{
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER

    public static partial class ParallelAsync
    {
        #region IEnumerable<T>        

        /// <summary>
        /// Runs the specified async method for each item of the input collection in a parallel/batched manner.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The collection of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="maxBatchSize">The maximum batch size to allow. Use 0 to default to <c>Environment.ProcessorCount</c></param>
        /// <param name="allowOutOfOrderProcessing">Boolean to allow out of order processing of input elements.</param>
        /// <param name="estimatedResultSize">The estimated size of the result collection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="collection"/> or <paramref name="func"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxBatchSize"/> is a negative number.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="allowOutOfOrderProcessing" /> flag allows you to specify wether to allow the out of order processing mode.
        /// This mode offers a performance improvement when the duration of each job varies, eg. due to network latency.
        /// When each run takes roughly the same amount of time, running in out of order mode can/will actually perform worse.
        /// As with all performance scenario's, do your own testing and pick what works for you.
        /// </para>
        /// <para>
        /// The <paramref name="estimatedResultSize"/> value is used to determine the size of the result <see cref="List{T}"/> to account for.
        /// The library will actually check if it can determine the size of <paramref name="collection"/>, without actually consuming it, using the <see cref="ListHelpers"/>.
        /// If it is unable to determine a size there, it will fall back to the value you specified in <paramref name="estimatedResultSize"/>.
        /// Setting this value to low, will mean a too small list will be allocated and you will have to pay a small performance hit for the resizing of the list during execution.
        /// </para>
        /// </remarks>
        public static IAsyncEnumerable<TResult> ForEachAsyncStream<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, Task<TResult>> func, int maxBatchSize = 0, bool allowOutOfOrderProcessing = false, int estimatedResultSize = 0, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var funcWithCancellationToken = WrapFunc(func);
            return ForEachAsyncStream<TResult, TIn>(collection, funcWithCancellationToken, maxBatchSize, allowOutOfOrderProcessing, estimatedResultSize, cancellationToken);
        }

        /// <summary>
        /// Runs the specified async method for each item of the input collection in a parallel/batched manner.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The collection of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="maxBatchSize">The maximum batch size to allow. Use 0 to default to <c>Environment.ProcessorCount</c></param>
        /// <param name="allowOutOfOrderProcessing">Boolean to allow out of order processing of input elements.</param>
        /// <param name="estimatedResultSize">The estimated size of the result collection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="collection"/> or <paramref name="func"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxBatchSize"/> is a negative number.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="allowOutOfOrderProcessing" /> flag allows you to specify wether to allow the out of order processing mode.
        /// This mode offers a performance improvement when the duration of each job varies, eg. due to network latency.
        /// When each run takes roughly the same amount of time, running in out of order mode can/will actually perform worse.
        /// As with all performance scenario's, do your own testing and pick what works for you.
        /// </para>
        /// <para>
        /// The <paramref name="estimatedResultSize"/> value is used to determine the size of the result <see cref="List{T}"/> to account for.
        /// The library will actually check if it can determine the size of <paramref name="collection"/>, without actually consuming it, using the <see cref="ListHelpers"/>.
        /// If it is unable to determine a size there, it will fall back to the value you specified in <paramref name="estimatedResultSize"/>.
        /// Setting this value to low, will mean a too small list will be allocated and you will have to pay a small performance hit for the resizing of the list during execution.
        /// </para>
        /// </remarks>
        public static IAsyncEnumerable<TResult> ForEachAsyncStream<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int maxBatchSize = 0, bool allowOutOfOrderProcessing = false, int estimatedResultSize = 0, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var maxBatchSizeToUse = DetermineBatchSizeToUse(maxBatchSize);

            if (maxBatchSizeToUse == 1)
            {
                return ForEachAsyncStreamImplUnbatchedAsync<TResult, TIn>(collection, func, estimatedResultSize, cancellationToken);
            }
            else if (allowOutOfOrderProcessing)
            {
                throw new NotImplementedException();
                //return ForEachAsyncStreamImplUnordered<TResult, TIn>(collection, func, maxBatchSizeToUse, estimatedResultSize, cancellationToken);
            }
            else
            {
                throw new NotImplementedException();
                //return ForEachAsyncStreamImplOrdered<TResult, TIn>(collection, func, maxBatchSizeToUse, estimatedResultSize, cancellationToken);
            }
        }

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
        private static async IAsyncEnumerable<TResult> ForEachAsyncStreamImplUnbatchedAsync<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int estimatedResultSize, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, 1, false, estimatedResultSize);

            int batchId = 0;
            foreach (var element in collection)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ParallelAsyncEventSource.Log.BatchStart(runId, batchId, 1);

                var resultElement = await func(element, cancellationToken).ConfigureAwait(false);

                ParallelAsyncEventSource.Log.BatchStop(runId, batchId);

                batchId++;

                yield return resultElement;
            }

            ParallelAsyncEventSource.Log.RunStop(runId);
        }

        #endregion IEnumerable<T>

        #region IAsyncEnumerable<T>

        /// <summary>
        /// Runs the specified async method for each item of the input collection in a parallel/batched manner.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The collection of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="maxBatchSize">The maximum batch size to allow. Use 0 to default to <c>Environment.ProcessorCount</c></param>
        /// <param name="allowOutOfOrderProcessing">Boolean to allow out of order processing of input elements.</param>
        /// <param name="estimatedResultSize">The estimated size of the result collection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="collection"/> or <paramref name="func"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxBatchSize"/> is a negative number.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="allowOutOfOrderProcessing" /> flag allows you to specify wether to allow the out of order processing mode.
        /// This mode offers a performance improvement when the duration of each job varies, eg. due to network latency.
        /// When each run takes roughly the same amount of time, running in out of order mode can/will actually perform worse.
        /// As with all performance scenario's, do your own testing and pick what works for you.
        /// </para>
        /// <para>
        /// The <paramref name="estimatedResultSize"/> value is used to determine the size of the result <see cref="List{T}"/> to account for.
        /// The library will actually check if it can determine the size of <paramref name="collection"/>, without actually consuming it, using the <see cref="ListHelpers"/>.
        /// If it is unable to determine a size there, it will fall back to the value you specified in <paramref name="estimatedResultSize"/>.
        /// Setting this value to low, will mean a too small list will be allocated and you will have to pay a small performance hit for the resizing of the list during execution.
        /// </para>
        /// </remarks>
        public static IAsyncEnumerable<TResult> ForEachAsyncStream<TResult, TIn>(IAsyncEnumerable<TIn> collection, Func<TIn, Task<TResult>> func, int maxBatchSize = 0, bool allowOutOfOrderProcessing = false, int estimatedResultSize = 0, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var funcWithCancellationToken = WrapFunc(func);
            return ForEachAsyncStream<TResult, TIn>(collection, funcWithCancellationToken, maxBatchSize, allowOutOfOrderProcessing, estimatedResultSize, cancellationToken);
        }

        /// <summary>
        /// Runs the specified async method for each item of the input collection in a parallel/batched manner.
        /// </summary>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <typeparam name="TIn">The input item type</typeparam>
        /// <param name="collection">The collection of items to use as input arguments</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="maxBatchSize">The maximum batch size to allow. Use 0 to default to <c>Environment.ProcessorCount</c></param>
        /// <param name="allowOutOfOrderProcessing">Boolean to allow out of order processing of input elements.</param>
        /// <param name="estimatedResultSize">The estimated size of the result collection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="collection"/> or <paramref name="func"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxBatchSize"/> is a negative number.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="allowOutOfOrderProcessing" /> flag allows you to specify wether to allow the out of order processing mode.
        /// This mode offers a performance improvement when the duration of each job varies, eg. due to network latency.
        /// When each run takes roughly the same amount of time, running in out of order mode can/will actually perform worse.
        /// As with all performance scenario's, do your own testing and pick what works for you.
        /// </para>
        /// <para>
        /// The <paramref name="estimatedResultSize"/> value is used to determine the size of the result <see cref="List{T}"/> to account for.
        /// The library will actually check if it can determine the size of <paramref name="collection"/>, without actually consuming it, using the <see cref="ListHelpers"/>.
        /// If it is unable to determine a size there, it will fall back to the value you specified in <paramref name="estimatedResultSize"/>.
        /// Setting this value to low, will mean a too small list will be allocated and you will have to pay a small performance hit for the resizing of the list during execution.
        /// </para>
        /// </remarks>
        public static IAsyncEnumerable<TResult> ForEachAsyncStream<TResult, TIn>(IAsyncEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int maxBatchSize = 0, bool allowOutOfOrderProcessing = false, int estimatedResultSize = 0, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            int maxBatchSizeToUse = DetermineBatchSizeToUse(maxBatchSize);

            if (maxBatchSizeToUse == 1)
            {
                return ForEachAsyncStreamImplUnbatchedAsync<TResult, TIn>(collection, func, estimatedResultSize, cancellationToken);
            }
            else if (allowOutOfOrderProcessing)
            {
                throw new NotImplementedException();
                //return ForEachAsyncStreamImplUnorderedAsync<TResult, TIn>(collection, func, maxBatchSizeToUse, estimatedResultSize, cancellationToken);
            }
            else
            {
                throw new NotImplementedException();
                //return ForEachAsyncStreamImplOrderedAsync<TResult, TIn>(collection, func, maxBatchSizeToUse, estimatedResultSize, cancellationToken);
            }
        }

        private static async IAsyncEnumerable<TResult> ForEachAsyncStreamImplUnbatchedAsync<TResult, TIn>(IAsyncEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int estimatedResultSize, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            long runId = ParallelAsyncEventSource.Log.GetRunId();
            ParallelAsyncEventSource.Log.RunStart(runId, 1, false, estimatedResultSize);

            var enumerator = collection.GetAsyncEnumerator(cancellationToken);
            try
            {
                var hasNext = true;
                int batchId = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);

                    if (!hasNext)
                    {
                        break;
                    }

                    ParallelAsyncEventSource.Log.BatchStart(runId, batchId, 1);

                    var element = enumerator.Current;
                    var resultElement = await func(element, cancellationToken).ConfigureAwait(false);

                    ParallelAsyncEventSource.Log.BatchStop(runId, batchId);

                    batchId++;

                    yield return resultElement;
                }
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }

            ParallelAsyncEventSource.Log.RunStop(runId);
        }

        #endregion IAsyncEnumerable<T>
    }

#endif //NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
}
