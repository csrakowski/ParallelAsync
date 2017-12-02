using System.Collections.Generic;

namespace CSRakowski.Parallel.Extensions
{
    /// <summary>
    /// Empty marker interface, used by the <see cref="ParallelAsyncEx"/>
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    public interface IParallelAsyncEnumerable<T> : IEnumerable<T>
    {
    }
}