using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CSRakowski.Parallel.Helpers
{
    /// <summary>
    /// A collection of helpers to get a <see cref="List{T}"/> of the right size
    /// </summary>
    /// <remarks>
    /// These helpers are used by the <see cref="ParallelAsync"/> class to get a <see cref="List{T}"/> big enough to hold the result collection
    /// </remarks>
    public static class ListHelpers
    {
        /// <summary>
        /// Get's an empty list with enough capacity to hold the entire collection, or with the fallback value size.
        /// </summary>
        /// <typeparam name="TResult">The type of the list elements</typeparam>
        /// <typeparam name="TIn">The type of the <paramref name="enumerable"/></typeparam>
        /// <param name="enumerable">The collection</param>
        /// <param name="estimatedResultSize">The fallback value</param>
        /// <returns>The list</returns>
        public static List<TResult> GetList<TResult, TIn>(IEnumerable<TIn> enumerable, int estimatedResultSize)
        {
            var size = DetermineResultSize(enumerable, estimatedResultSize);
            return GetList<TResult>(size);
        }

        /// <summary>
        /// Attempt get the size of the <paramref name="enumerable"/>, without actually consuming it.
        /// Falls back to <paramref name="estimatedResultSize"/> if that is not possible.
        /// </summary>
        /// <typeparam name="T">The type of the collection</typeparam>
        /// <param name="enumerable">The collection</param>
        /// <param name="estimatedResultSize">The fallback value</param>
        /// <returns>The size of the collection, or the fallback value</returns>
        public static int DetermineResultSize<T>(IEnumerable<T> enumerable, int estimatedResultSize)
        {
            switch (enumerable)
            {
                case null:
                    return 0;
                case ICollection<T> col:
                    return col.Count;
                case ICollection col:
                    return col.Count;
                case IReadOnlyCollection<T> col:
                    return col.Count;
                default:
                    return estimatedResultSize;
            }
        }

        /// <summary>
        /// Get's an empty list with the specified capacity
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="capacity">The capacity for the list</param>
        /// <returns>The list</returns>
        /// <remarks>
        /// Basically just calls the constructor overload with the specified capacity
        /// </remarks>
        public static List<T> GetList<T>(int capacity)
        {
            if (capacity > 0)
            {
                return new List<T>(capacity);
            }
            else
            {
                return new List<T>();
            }
        }
    }
}
