using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Extensions
{
    #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER

    /// <summary>
    /// Extension methods to allow using the functionalities of <see cref="ParallelAsync"/> with a fluent syntax
    /// </summary>
    public static partial class ParallelAsyncEx
    {
        #region ForEachAsyncStream overloads

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
        public static IAsyncEnumerable<TResult> ForEachAsyncStream<TInput, TResult>(this IParallelAsyncEnumerable<TInput> parallelAsync, Func<TInput, Task<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            if (obj.IsAsyncEnumerable)
            {
                return ParallelAsync.ForEachAsyncStream(obj.AsyncEnumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, obj.EstimatedResultSize, cancellationToken);
            }
            else
            {
                return ParallelAsync.ForEachAsyncStream(obj.Enumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, obj.EstimatedResultSize, cancellationToken);
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
        public static IAsyncEnumerable<TResult> ForEachAsyncStream<TInput, TResult>(this IParallelAsyncEnumerable<TInput> parallelAsync, Func<TInput, CancellationToken, Task<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var obj = EnsureValidEnumerable(parallelAsync);

            if (obj.IsAsyncEnumerable)
            {
                return ParallelAsync.ForEachAsyncStream(obj.AsyncEnumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, obj.EstimatedResultSize, cancellationToken);
            }
            else
            {
                return ParallelAsync.ForEachAsyncStream(obj.Enumerable, func, obj.MaxDegreeOfParallelism, obj.AllowOutOfOrderProcessing, obj.EstimatedResultSize, cancellationToken);
            }
        }

        #endregion ForEachAsyncStream overloads
    }

    #endif //NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER

}
