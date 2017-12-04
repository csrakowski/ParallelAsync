using System;
using System.Collections;
using System.Collections.Generic;

namespace CSRakowski.Parallel.Extensions
{
    /// <summary>
    /// Internal helper class that wraps an <see cref="IEnumerable{T}"/> and configuration values used by <see cref="ParallelAsyncEx"/>
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    internal class ParallelAsyncEnumerable<T> : IParallelAsyncEnumerable<T>
    {
        /// <summary>
        /// The wrapped <see cref="IEnumerable{T}"/>
        /// </summary>
        internal readonly IEnumerable<T> Enumerable;

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
        internal ParallelAsyncEnumerable(in IEnumerable<T> enumerable)
        {
            Enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
            MaxDegreeOfParallelism = 0;
            EstimatedResultSize = 0;
            AllowOutOfOrderProcessing = false;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Enumerable).GetEnumerator();
        }
    }
}