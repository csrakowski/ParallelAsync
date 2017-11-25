﻿using CSRakowski.Parallel.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel
{
    /// <summary>
    /// Helper class to assist with running async workloads in a parallel/batched manner.
    /// </summary>
    public static partial class ParallelAsync
    {
        #region Helpers

        /// <summary>
        /// Determine the batch size to use
        /// </summary>
        /// <param name="maxBatchSize">The maximum batch size to allow. Use 0 to default to <c>Environment.ProcessorCount</c></param>
        /// <returns>The batch size to use</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxBatchSize"/> is a negative number.</exception>
        private static int DetermineBatchSizeToUse(int maxBatchSize)
        {
            if (maxBatchSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBatchSize), "Needs to be a positive number, or 0 to default to the CPU count");
            }
            else if (maxBatchSize == 0)
            {
                return Environment.ProcessorCount;
            }
            else
            {
                return maxBatchSize;
            }
        }

        #endregion Helpers

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
        public static Task<IEnumerable<TResult>> ForEachAsync<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, Task<TResult>> func, int maxBatchSize = 0, bool allowOutOfOrderProcessing = false, int estimatedResultSize = 0, CancellationToken cancellationToken = default)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            maxBatchSize = DetermineBatchSizeToUse(maxBatchSize);

            if (maxBatchSize == 1)
            {
                return ForEachAsyncImplUnbatched<TResult, TIn>(collection, cancellationToken, estimatedResultSize, func);
            }
            else if (allowOutOfOrderProcessing)
            {
                return ForEachAsyncImplUnordered<TResult, TIn>(collection, cancellationToken, maxBatchSize, estimatedResultSize, func);
            }
            else
            {
                return ForEachAsyncImplOrdered<TResult, TIn>(collection, cancellationToken, maxBatchSize, estimatedResultSize, func);
            }
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
        public static Task<IEnumerable<TResult>> ForEachAsync<TResult, TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task<TResult>> func, int maxBatchSize = 0, bool allowOutOfOrderProcessing = false, int estimatedResultSize = 0, CancellationToken cancellationToken = default)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            maxBatchSize = DetermineBatchSizeToUse(maxBatchSize);

            if (maxBatchSize == 1)
            {
                return ForEachAsyncImplUnbatched<TResult, TIn>(collection, cancellationToken, estimatedResultSize, func);
            }
            else if (allowOutOfOrderProcessing)
            {
                return ForEachAsyncImplUnordered<TResult, TIn>(collection, cancellationToken, maxBatchSize, estimatedResultSize, func);
            }
            else
            {
                return ForEachAsyncImplOrdered<TResult, TIn>(collection, cancellationToken, maxBatchSize, estimatedResultSize, func);
            }
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
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task"/> signaling completion.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="collection"/> or <paramref name="func"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxBatchSize"/> is a negative number.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="allowOutOfOrderProcessing" /> flag allows you to specify wether to allow the out of order processing mode.
        /// This mode offers a performance improvement when the duration of each job varies, eg. due to network latency.
        /// When each run takes roughly the same amount of time, running in out of order mode can/will actually perform worse.
        /// As with all performance scenario's, do your own testing and pick what works for you.
        /// </para>
        /// </remarks>
        public static Task ForEachAsync<TIn>(IEnumerable<TIn> collection, Func<TIn, Task> func, int maxBatchSize = 0, bool allowOutOfOrderProcessing = false, CancellationToken cancellationToken = default)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            maxBatchSize = DetermineBatchSizeToUse(maxBatchSize);

            if (maxBatchSize == 1)
            {
                return ForEachAsyncImplUnbatched<TIn>(collection, cancellationToken, func);
            }
            else if (allowOutOfOrderProcessing)
            {
                return ForEachAsyncImplUnordered<TIn>(collection, cancellationToken, maxBatchSize, func);
            }
            else
            {
                return ForEachAsyncImplOrdered<TIn>(collection, cancellationToken, maxBatchSize, func);
            }
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
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task"/> signaling completion.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="collection"/> or <paramref name="func"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxBatchSize"/> is a negative number.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="allowOutOfOrderProcessing" /> flag allows you to specify wether to allow the out of order processing mode.
        /// This mode offers a performance improvement when the duration of each job varies, eg. due to network latency.
        /// When each run takes roughly the same amount of time, running in out of order mode can/will actually perform worse.
        /// As with all performance scenario's, do your own testing and pick what works for you.
        /// </para>
        /// </remarks>
        public static Task ForEachAsync<TIn>(IEnumerable<TIn> collection, Func<TIn, CancellationToken, Task> func, int maxBatchSize = 0, bool allowOutOfOrderProcessing = false, CancellationToken cancellationToken = default)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            maxBatchSize = DetermineBatchSizeToUse(maxBatchSize);

            if (maxBatchSize == 1)
            {
                return ForEachAsyncImplUnbatched<TIn>(collection, cancellationToken, func);
            }
            else if (allowOutOfOrderProcessing)
            {
                return ForEachAsyncImplUnordered<TIn>(collection, cancellationToken, maxBatchSize, func);
            }
            else
            {
                return ForEachAsyncImplOrdered<TIn>(collection, cancellationToken, maxBatchSize, func);
            }
        }
    }
}