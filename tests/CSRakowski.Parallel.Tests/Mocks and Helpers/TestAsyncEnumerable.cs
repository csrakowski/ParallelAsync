using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Tests.Helpers
{
    public static class TestAsyncEnumerableEx
    {
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> enumerable)
        {
            return new TestAsyncEnumerable<T>(enumerable);
        }
    }

    public class TestAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _enumerable;

        internal TestAsyncEnumerable(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator() =>
            new TestAsyncEnumerator<T>(_enumerable.GetEnumerator());
    }

    public struct TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        internal TestAsyncEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public Task<bool> MoveNextAsync() =>
            Task.FromResult(_enumerator.MoveNext());

        public T Current => _enumerator.Current;

        public Task DisposeAsync()
        {
            _enumerator.Dispose();
            return TaskHelper.CompletedTask;
        }
    }
}
