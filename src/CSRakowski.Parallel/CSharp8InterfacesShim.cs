using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime;
using System.Runtime.CompilerServices;

/*
Taken from the proposal on 2018-01-19
Source: https://github.com/dotnet/csharplang/blob/master/proposals/async-streams.md

*/


namespace System
{
    public interface IAsyncDisposable
    {
        Task DisposeAsync();
    }
}

namespace System.Collections.Generic
{
    public interface IAsyncEnumerable<out T>
    {
        IAsyncEnumerator<T> GetAsyncEnumerator();
    }

    public interface IAsyncEnumerator<out T> : IAsyncDisposable
    {
        Task<bool> MoveNextAsync();
        T Current { get; }
    }
}

// Approximate implementation, omitting arg validation and the like
namespace System.Threading.Tasks
{
    public static class AsyncEnumerableExtensions
    {
        public static ConfiguredAsyncEnumerable<T> ConfigureAwait<T>(this IAsyncEnumerable<T> enumerable, bool continueOnCapturedContext) =>
            new ConfiguredAsyncEnumerable<T>(enumerable, continueOnCapturedContext);

        public struct ConfiguredAsyncEnumerable<T>
        {
            private readonly IAsyncEnumerable<T> _enumerable;
            private readonly bool _continueOnCapturedContext;

            internal ConfiguredAsyncEnumerable(IAsyncEnumerable<T> enumerable, bool continueOnCapturedContext)
            {
                _enumerable = enumerable;
                _continueOnCapturedContext = continueOnCapturedContext;
            }

            public ConfiguredAsyncEnumerator<T> GetAsyncEnumerator() =>
                new ConfiguredAsyncEnumerator<T>(_enumerable.GetAsyncEnumerator(), _continueOnCapturedContext);
        }

        public struct ConfiguredAsyncEnumerator<T>
        {
            private readonly IAsyncEnumerator<T> _enumerator;
            private readonly bool _continueOnCapturedContext;

            internal ConfiguredAsyncEnumerator(IAsyncEnumerator<T> enumerator, bool continueOnCapturedContext)
            {
                _enumerator = enumerator;
                _continueOnCapturedContext = continueOnCapturedContext;
            }

            public ConfiguredTaskAwaitable<bool> MoveNextAsync() =>
                _enumerator.MoveNextAsync().ConfigureAwait(_continueOnCapturedContext);

            public T Current => _enumerator.Current;

            public ConfiguredTaskAwaitable DisposeAsync() =>
                _enumerator.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
        }
    }
}

namespace CSRakowski.Parallel.Helpers
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
#if NET45 || NETSTANDARD1_1
            return Task.FromResult(true);
#else
            return Task.CompletedTask;
#endif
        }
    }
}