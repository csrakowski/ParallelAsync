using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Extensions
{
    public static class ParallelAsyncEx
    {
        #region With.. Configuration methods

        public static IParallelAsyncEnumerable<T> AsParallelAsync<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            return new ParallelAsyncEnumerable<T>(enumerable);
        }

        public static IParallelAsyncEnumerable<T> WithMaxDegreeOfParallelism<T>(this IParallelAsyncEnumerable<T> parallelAsync, int maxDegreeOfParallelism)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            if (maxDegreeOfParallelism < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), "Must be either 0 or a positive number");
            }

            obj.MaxDegreeOfParallelism = maxDegreeOfParallelism;

            return obj;
        }

        public static IParallelAsyncEnumerable<T> WithEstimatedResultSize<T>(this IParallelAsyncEnumerable<T> parallelAsync, int estimatedResultSize)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            if (estimatedResultSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(estimatedResultSize), "Must be either 0 or a positive number");
            }

            obj.EstimatedResultSize = estimatedResultSize;

            return obj;
        }

        public static IParallelAsyncEnumerable<T> WithOutOfOrderProcessing<T>(this IParallelAsyncEnumerable<T> parallelAsync, bool allowOutOfOrderProcessing)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            obj.AllowOutOfOrderProcessing = allowOutOfOrderProcessing;

            return obj;
        }

        #endregion With.. Configuration methods

        #region ForEachAsync overloads

        public static Task<IEnumerable<TResult>> ForEachAsync<TInput, TResult>(this IParallelAsyncEnumerable<TInput> parallelAsync, Func<TInput, Task<TResult>> func, CancellationToken cancellationToken = default)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            return ParallelAsync.ForEachAsync(obj.Enumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, obj.EstimatedResultSize, cancellationToken);
        }

        public static Task<IEnumerable<TResult>> ForEachAsync<TInput, TResult>(this IParallelAsyncEnumerable<TInput> parallelAsync, Func<TInput, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken = default)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            return ParallelAsync.ForEachAsync(obj.Enumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, obj.EstimatedResultSize, cancellationToken);
        }

        public static Task ForEachAsync<TInput>(this IParallelAsyncEnumerable<TInput> parallelAsync, Func<TInput, Task> func, CancellationToken cancellationToken = default)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            return ParallelAsync.ForEachAsync(obj.Enumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, cancellationToken);
        }

        public static Task ForEachAsync<TInput>(this IParallelAsyncEnumerable<TInput> parallelAsync, Func<TInput, CancellationToken, Task> func, CancellationToken cancellationToken = default)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            return ParallelAsync.ForEachAsync(obj.Enumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, cancellationToken);
        }

        #endregion ForEachAsync overloads

        #region Internal Helpers

        private static ParallelAsyncEnumerable<T> EnsureValidEnumerable<T>(IParallelAsyncEnumerable<T> parallelAsync)
        {
            var obj = parallelAsync as ParallelAsyncEnumerable<T>;
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(parallelAsync));
            }
            return obj;
        }

        #endregion Internal Helpers
    }
}
