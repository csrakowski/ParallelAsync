using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.ParallelAsync
{
    public static partial class Parallel
    {
        private static async Task<IEnumerable<TResult>> ForEachAsyncImplUnbatched<TResult, TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int estimatedResultSize, Func<TIn, Task<TResult>> func)
        {
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;

                while (hasNext && !cancellationToken.IsCancellationRequested)
                {
                    hasNext = enumerator.MoveNext();

                    if (!hasNext)
                    {
                        break;
                    }

                    var element = enumerator.Current;

                    var resultElement = await func(element).ConfigureAwait(false);

                    result.Add(resultElement);
                }
            }

            return result;
        }

        private static async Task<IEnumerable<TResult>> ForEachAsyncImplUnbatched<TResult, TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, int estimatedResultSize, Func<TIn, CancellationToken, Task<TResult>> func)
        {
            var result = ListHelpers.GetList<TResult, TIn>(collection, estimatedResultSize);

            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;

                while (hasNext && !cancellationToken.IsCancellationRequested)
                {
                    hasNext = enumerator.MoveNext();

                    if (!hasNext)
                    {
                        break;
                    }

                    var element = enumerator.Current;

                    var resultElement = await func(element, cancellationToken).ConfigureAwait(false);

                    result.Add(resultElement);
                }
            }

            return result;
        }

        private static async Task ForEachAsyncImplUnbatched<TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, Func<TIn, Task> func)
        {
            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;

                while (hasNext && !cancellationToken.IsCancellationRequested)
                {
                    hasNext = enumerator.MoveNext();

                    if (!hasNext)
                    {
                        break;
                    }

                    var element = enumerator.Current;

                    await func(element).ConfigureAwait(false);
                }
            }
        }

        private static async Task ForEachAsyncImplUnbatched<TIn>(IEnumerable<TIn> collection, CancellationToken cancellationToken, Func<TIn, CancellationToken, Task> func)
        {
            using (var enumerator = collection.GetEnumerator())
            {
                var hasNext = true;

                while (hasNext && !cancellationToken.IsCancellationRequested)
                {
                    hasNext = enumerator.MoveNext();

                    if (!hasNext)
                    {
                        break;
                    }

                    var element = enumerator.Current;

                    await func(element, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
