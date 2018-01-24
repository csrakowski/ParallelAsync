using System;
using System.Collections;
using System.Collections.Generic;

namespace CSRakowski.Parallel.Extensions
{
    /// <summary>
    /// Internal helper class that wraps an <see cref="IEnumerable{T}"/> or <see cref="IAsyncEnumerable{T}"/>, and configuration values used by <see cref="ParallelAsyncEx"/>
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    internal class ParallelAsyncEnumerable<T> : IParallelAsyncEnumerable<T>
    {
        /// <summary>
        /// The wrapped <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <remarks>
        /// Will be <c>null</c> if <see cref="IsAsyncEnumerable"/> is <c>true</c>
        /// </remarks>
        internal readonly IEnumerable<T> Enumerable;

        /// <summary>
        /// The wrapped <see cref="IAsyncEnumerable{T}"/>
        /// </summary>
        /// <remarks>
        /// Will be <c>null</c> if <see cref="IsAsyncEnumerable"/> is <c>false</c>
        /// </remarks>
        internal readonly IAsyncEnumerable<T> AsyncEnumerable;

        /// <summary>
        /// Indicates that the wrapped collection is an <see cref="IAsyncEnumerable{T}"/>
        /// </summary>
        internal bool IsAsyncEnumerable => AsyncEnumerable != null;

        /// <summary>
        /// The maximum batch size to allow
        /// </summary>
        internal int MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// The estimated result size
        /// </summary>
        internal int EstimatedResultSize { get; set; }

        /// <summary>
        /// Whether or not to allow out of order processing
        /// </summary>
        internal bool AllowOutOfOrderProcessing { get; set; }

        /// <summary>
        /// Instantiates a new <see cref="ParallelAsyncEnumerable{T}"/> that wraps the specified <paramref name="enumerable"/>
        /// </summary>
        /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to wrap</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumerable"/> is <c>null</c></exception>
        internal ParallelAsyncEnumerable(IEnumerable<T> enumerable)
        {
            Enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
            MaxDegreeOfParallelism = 0;
            EstimatedResultSize = 0;
            AllowOutOfOrderProcessing = false;
        }

        /// <summary>
        /// Instantiates a new <see cref="ParallelAsyncEnumerable{T}"/> that wraps the specified <paramref name="enumerable"/>
        /// </summary>
        /// <param name="enumerable">The <see cref="IAsyncEnumerable{T}"/> to wrap</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumerable"/> is <c>null</c></exception>
        internal ParallelAsyncEnumerable(IAsyncEnumerable<T> enumerable)
        {
            AsyncEnumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
            MaxDegreeOfParallelism = 0;
            EstimatedResultSize = 0;
            AllowOutOfOrderProcessing = false;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            if (IsAsyncEnumerable)
            {
                if (AsyncEnumerable is IEnumerable<T> enumerable)
                {
                    return enumerable.GetEnumerator();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            return Enumerable.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (IsAsyncEnumerable)
            {
                if (AsyncEnumerable is IEnumerable enumerable)
                {
                    return enumerable.GetEnumerator();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            return ((IEnumerable)Enumerable).GetEnumerator();
        }
    }
}