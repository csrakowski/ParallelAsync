using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSRakowski.Parallel;
using CSRakowski.Parallel.Helpers;
using CSRakowski.Parallel.Tests.Helpers;
using CSRakowski.AsyncStreamsPreparations;
using CSRakowski.Parallel.Extensions;

namespace CSRakowski.Parallel.Tests
{
    [Collection("ParallelAsync AsyncStreams Tests")]
    public class ParallelAsyncTests_AsyncStreams
    {
        [Fact]
        public async Task ParallelAsync_Can_Process_IEnumerable_Streaming()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, (el) => Task.FromResult(el * 2), maxBatchSize: 1, estimatedResultSize: 10);

            int i = 0;
            await foreach (var el in asyncEnumerable)
            {
                var expected = 2 * (1 + i);
                Assert.Equal(expected, el);
                i++;
            }
        }

        [Fact]
        public async Task ParallelAsync_Can_Process_IAsyncEnumerable_Streaming()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.AsAsyncEnumerable();

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, (el) => Task.FromResult(el * 2), maxBatchSize: 1, estimatedResultSize: 10);

            int i = 0;
            await foreach (var el in asyncEnumerable)
            {
                var expected = 2 * (1 + i);
                Assert.Equal(expected, el);
                i++;
            }
        }

        [Fact]
        public async Task ParallelAsync_Can_Process_IEnumerable_Streaming_Using_Default_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            int numberOfCalls = 0;

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, async (el, ct) =>
            {
                await Task.Delay(500, ct);
                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, maxBatchSize: 4, estimatedResultSize: 10);

            await foreach (var result in asyncEnumerable)
            {
                Assert.True((result > 0 && result < 11), "Expected a result between 1 and 10");
            }

            Assert.Equal(10, numberOfCalls);
        }

        [Fact]
        public async Task ParallelAsync_Can_Process_IAsyncEnumerable_Streaming_Using_Default_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.AsAsyncEnumerable();

            int numberOfCalls = 0;

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, async (el, ct) =>
            {
                await Task.Delay(500, ct);
                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, maxBatchSize: 4, estimatedResultSize: 10);

            await foreach (var result in asyncEnumerable)
            {
                Assert.True((result > 0 && result < 11), "Expected a result between 1 and 10");
            }

            Assert.Equal(10, numberOfCalls);
        }

        [Fact]
        public async Task ParallelAsync_Can_Process_IEnumerable_Streaming_Using_Provided_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, async (el, ct) =>
            {
                await Task.Delay(500);

                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, cancellationToken: cancellationToken, maxBatchSize: 1, estimatedResultSize: 10);

            var numberOfResults = 0;
            await foreach (var result in asyncEnumerable)
            {
                Assert.True((result > 0 && result < 11), "Expected a result between 1 and 10");
                numberOfResults++;
            }

            Assert.True(numberOfCalls < 10);
            Assert.True(numberOfResults <= numberOfCalls, $"Expected less than, or equal to, {numberOfCalls}, but got {numberOfResults}");

            cts.Dispose();
        }

        [Fact]
        public async Task ParallelAsync_Can_Process_IAsyncEnumerable_Streaming_Using_Provided_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.AsAsyncEnumerable();

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, async (el, ct) =>
            {
                await Task.Delay(500);

                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, cancellationToken: cancellationToken, maxBatchSize: 1, estimatedResultSize: 10);

            var numberOfResults = 0;
            await foreach (var result in asyncEnumerable)
            {
                Assert.True((result > 0 && result < 11), "Expected a result between 1 and 10");
                numberOfResults++;
            }

            Assert.True(numberOfCalls < 10);
            Assert.True(numberOfResults <= numberOfCalls, $"Expected less than, or equal to, {numberOfCalls}, but got {numberOfResults}");

            cts.Dispose();
        }

        [Fact]
        public async Task ParallelAsync_Throws_On_Invalid_Inputs()
        {
            var empty = new int[0];
            IEnumerable<int> nullEnumerable = null;

            var emptyAsync = new int[0].AsAsyncEnumerable();
            IAsyncEnumerable<int> nullAsyncEnumerable = null;

            var ex1 = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(nullEnumerable, (e) => Task.FromResult(e)))
                {
                }
            });
            var ex2 = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(nullEnumerable, (e, ct) => Task.FromResult(e)))
                {
                }
            });

            var ex3 = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(nullAsyncEnumerable, (e) => Task.FromResult(e)))
                {
                }
            });
            var ex4 = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(nullAsyncEnumerable, (e, ct) => Task.FromResult(e)))
                {
                }
            });

            var ex5 = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(empty, (Func<int, Task<int>>)null))
                {
                }
            });
            var ex6 = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(empty, (Func<int, CancellationToken, Task<int>>)null))
                {
                }
            });

            var ex7 = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(emptyAsync, (Func<int, Task<int>>)null))
                {
                }
            });
            var ex8 = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(emptyAsync, (Func<int, CancellationToken, Task<int>>)null))
                {
                }
            });

            var ex9 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(empty, (e, ct) => Task.FromResult(e), -1))
                {
                }
            });
            var ex10 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(emptyAsync, (e, ct) => Task.FromResult(e), -1))
                {
                }
            });

            Assert.Equal("collection", ex1.ParamName);
            Assert.Equal("collection", ex2.ParamName);
            Assert.Equal("collection", ex3.ParamName);
            Assert.Equal("collection", ex4.ParamName);

            Assert.Equal("func", ex5.ParamName);
            Assert.Equal("func", ex6.ParamName);
            Assert.Equal("func", ex7.ParamName);
            Assert.Equal("func", ex8.ParamName);

            Assert.Equal("maxBatchSize", ex9.ParamName);
            Assert.Equal("maxBatchSize", ex10.ParamName);
        }

        [Fact]
        public async Task ParallelAsync_Can_Chain_Together_AsyncStreams()
        {
            var input = Enumerable.Range(1, 40).ToList().AsAsyncEnumerable();

            var intermediateResult = ParallelAsync.ForEachAsyncStream(input, (el) => Task.FromResult(el * 2), maxBatchSize: 3, estimatedResultSize: 10);

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(intermediateResult, (el) => Task.FromResult(el * 2), maxBatchSize: 3, estimatedResultSize: 10);

            int i = 0;
            await foreach (var el in asyncEnumerable)
            {
                var expected = 4 * (1 + i);
                Assert.Equal(expected, el);
                i++;
            }
        }
    }
}
