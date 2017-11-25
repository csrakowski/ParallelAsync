using System;
using System.Collections;
using System.Collections.Generic;

namespace CSRakowski.Parallel.Extensions
{
    internal class ParallelAsyncEnumerable<T> : IParallelAsyncEnumerable<T>
    {
        internal readonly IEnumerable<T> Enumerable;

        internal int MaxDegreeOfParallelism { get; set; }

        internal int EstimatedResultSize { get; set; }

        internal bool AllowOutOfOrderProcessing { get; set; }


        public ParallelAsyncEnumerable(IEnumerable<T> enumerable)
        {
            Enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
            MaxDegreeOfParallelism = 0;
            EstimatedResultSize = 0;
            AllowOutOfOrderProcessing = false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Enumerable).GetEnumerator();
        }
    }
}