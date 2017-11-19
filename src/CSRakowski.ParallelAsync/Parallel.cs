using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.ParallelAsync
{
    public static partial class Parallel
    {
        #region Helpers

        /// <summary>
        /// Determine the batch size to use
        /// </summary>
        /// <param name="maxBatchSize">The max batch size to allow. Use 0 to default to <c>Environment.ProcessorCount</c></param>
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
