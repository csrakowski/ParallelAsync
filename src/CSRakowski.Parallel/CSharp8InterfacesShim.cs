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
