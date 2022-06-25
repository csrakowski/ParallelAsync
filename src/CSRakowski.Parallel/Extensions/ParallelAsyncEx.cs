using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Extensions
{
    /// <summary>
    /// Extension methods to allow using the functionalities of <see cref="ParallelAsync"/> with a fluent syntax
    /// </summary>
    public static partial class ParallelAsyncEx
    {
        #region With.. Configuration methods

        /// <summary>
        /// Wraps the collection as an <see cref="IParallelAsyncEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="enumerable">The collection to wrap</param>
        /// <returns>The wrapped collection</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumerable"/> is <c>null</c>.</exception>
        public static IParallelAsyncEnumerable<T> AsParallelAsync<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (enumerable is IParallelAsyncEnumerable<T> parallelAsync)
            {
                return parallelAsync;
            }

            return new ParallelAsyncEnumerable<T>(enumerable);
        }

        /// <summary>
        /// Wraps the collection as an <see cref="IParallelAsyncEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="enumerable">The collection to wrap</param>
        /// <returns>The wrapped collection</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumerable"/> is <c>null</c>.</exception>
        public static IParallelAsyncEnumerable<T> AsParallelAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (enumerable is IParallelAsyncEnumerable<T> parallelAsync)
            {
                return parallelAsync;
            }

            return new ParallelAsyncEnumerable<T>(enumerable);
        }

        /// <summary>
        /// Configure a maximum batch size to allow.
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="parallelAsync">The <see cref="IParallelAsyncEnumerable{T}"/></param>
        /// <param name="maxDegreeOfParallelism">The maximum batch size to allow. Use 0 to default to <c>Environment.ProcessorCount</c></param>
        /// <returns>The configured collections</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxDegreeOfParallelism"/> is a negative number.</exception>
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

        /// <summary>
        /// Configured the estimated result size,
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="parallelAsync">The <see cref="IParallelAsyncEnumerable{T}"/></param>
        /// <param name="estimatedResultSize">The estimated size of the result collection.</param>
        /// <returns>The configured collections</returns>
        /// <remarks>
        /// The <paramref name="estimatedResultSize"/> value is used to determine the size of the result <see cref="List{T}"/> to account for.
        /// The library will actually check if it can determine the size of <paramref name="parallelAsync"/>, without actually consuming it, using the <see cref="Helpers.ListHelpers"/>.
        /// If it is unable to determine a size there, it will fall back to the value you specified in <paramref name="estimatedResultSize"/>.
        /// Setting this value to low, will mean a too small list will be allocated and you will have to pay a small performance hit for the resizing of the list during execution.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="estimatedResultSize"/> is a negative number.</exception>
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

        /// <summary>
        /// Configure whether or not to allow out of order processing.
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="parallelAsync">The <see cref="IParallelAsyncEnumerable{T}"/></param>
        /// <param name="allowOutOfOrderProcessing">Boolean to allow out of order processing of input elements.</param>
        /// <returns>The configured collections</returns>
        /// <remarks>
        /// The <paramref name="allowOutOfOrderProcessing" /> flag allows you to specify wether to allow the out of order processing mode.
        /// This mode offers a performance improvement when the duration of each job varies, eg. due to network latency.
        /// When each run takes roughly the same amount of time, running in out of order mode can/will actually perform worse.
        /// As with all performance scenario's, do your own testing and pick what works for you.
        /// </remarks>
        public static IParallelAsyncEnumerable<T> WithOutOfOrderProcessing<T>(this IParallelAsyncEnumerable<T> parallelAsync, bool allowOutOfOrderProcessing)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            obj.AllowOutOfOrderProcessing = allowOutOfOrderProcessing;

            return obj;
        }

        #endregion With.. Configuration methods

        #region ForEachAsync overloads

        /// <summary>
        /// Runs the specified async method for each item of the input collection in a parallel/batched manner.
        /// </summary>
        /// <typeparam name="TInput">The input item type</typeparam>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <param name="parallelAsync">The <see cref="IParallelAsyncEnumerable{T}"/> to process</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="parallelAsync"/> or <paramref name="func"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the configured maximum batch size is a negative number.</exception>
        public static Task<IEnumerable<TResult>> ForEachAsync<TInput, TResult>(this IParallelAsyncEnumerable<TInput> parallelAsync, Func<TInput, Task<TResult>> func, CancellationToken cancellationToken = default)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            if (obj.IsAsyncEnumerable)
            {
                return ParallelAsync.ForEachAsync(obj.AsyncEnumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, obj.EstimatedResultSize, cancellationToken);
            }
            else
            {
                return ParallelAsync.ForEachAsync(obj.Enumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, obj.EstimatedResultSize, cancellationToken);
            }
        }

        /// <summary>
        /// Runs the specified async method for each item of the input collection in a parallel/batched manner.
        /// </summary>
        /// <typeparam name="TInput">The input item type</typeparam>
        /// <typeparam name="TResult">The result item type</typeparam>
        /// <param name="parallelAsync">The <see cref="IParallelAsyncEnumerable{T}"/> to process</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="parallelAsync"/> or <paramref name="func"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the configured maximum batch size is a negative number.</exception>
        public static Task<IEnumerable<TResult>> ForEachAsync<TInput, TResult>(this IParallelAsyncEnumerable<TInput> parallelAsync, Func<TInput, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken = default)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            if (obj.IsAsyncEnumerable)
            {
                return ParallelAsync.ForEachAsync(obj.AsyncEnumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, obj.EstimatedResultSize, cancellationToken);
            }
            else
            {
                return ParallelAsync.ForEachAsync(obj.Enumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, obj.EstimatedResultSize, cancellationToken);
            }
        }

        /// <summary>
        /// Runs the specified async method for each item of the input collection in a parallel/batched manner.
        /// </summary>
        /// <typeparam name="TInput">The input item type</typeparam>
        /// <param name="parallelAsync">The <see cref="IParallelAsyncEnumerable{T}"/> to process</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="parallelAsync"/> or <paramref name="func"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the configured maximum batch size is a negative number.</exception>
        public static Task ForEachAsync<TInput>(this IParallelAsyncEnumerable<TInput> parallelAsync, Func<TInput, Task> func, CancellationToken cancellationToken = default)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            if (obj.IsAsyncEnumerable)
            {
                return ParallelAsync.ForEachAsync(obj.AsyncEnumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, cancellationToken);
            }
            else
            {
                return ParallelAsync.ForEachAsync(obj.Enumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, cancellationToken);
            }
        }

        /// <summary>
        /// Runs the specified async method for each item of the input collection in a parallel/batched manner.
        /// </summary>
        /// <typeparam name="TInput">The input item type</typeparam>
        /// <param name="parallelAsync">The <see cref="IParallelAsyncEnumerable{T}"/> to process</param>
        /// <param name="func">The async method to run for each item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The results of the operations</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="parallelAsync"/> or <paramref name="func"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the configured maximum batch size is a negative number.</exception>
        public static Task ForEachAsync<TInput>(this IParallelAsyncEnumerable<TInput> parallelAsync, Func<TInput, CancellationToken, Task> func, CancellationToken cancellationToken = default)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            if (obj.IsAsyncEnumerable)
            {
                return ParallelAsync.ForEachAsync(obj.AsyncEnumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, cancellationToken);
            }
            else
            {
                return ParallelAsync.ForEachAsync(obj.Enumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, cancellationToken);
            }
        }

        #endregion ForEachAsync overloads

        #region Internal Helpers

        /// <summary>
        /// Ensure that the <paramref name="parallelAsync"/> passed in is a valid <see cref="ParallelAsyncEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="parallelAsync">The <see cref="IParallelAsyncEnumerable{T}"/> to check</param>
        /// <returns>The <see cref="ParallelAsyncEnumerable{T}"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parallelAsync"/> is not a valid <see cref="ParallelAsyncEnumerable{T}"/></exception>
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
