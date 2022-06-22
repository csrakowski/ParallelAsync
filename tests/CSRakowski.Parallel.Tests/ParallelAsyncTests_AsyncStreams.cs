using NUnit.Framework;
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

namespace CSRakowski.Parallel.Tests
{
    #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER

    [TestFixture, Category("ParallelAsync AsyncStreams Tests")]
    public class ParallelAsyncTests_AsyncStreams
    {
        [Test]
        public async Task ParallelAsync_Can_Process_IEnumerable_Streaming()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, (el) => Task.FromResult(el * 2), maxBatchSize: 1, estimatedResultSize: 10);

            int i = 0;
            await foreach (var el in asyncEnumerable)
            {
                var expected = 2 * (1 + i);
                Assert.AreEqual(expected, el);
                i++;
            }
        }

        [Test]
        public async Task ParallelAsync_Can_Process_IAsyncEnumerable_Streaming()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.AsAsyncEnumerable();

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, (el) => Task.FromResult(el * 2), maxBatchSize: 1, estimatedResultSize: 10);

            int i = 0;
            await foreach (var el in asyncEnumerable)
            {
                var expected = 2 * (1 + i);
                Assert.AreEqual(expected, el);
                i++;
            }
        }

        [Test]
        public async Task ParallelAsync_Can_Process_IEnumerable_Streaming_Using_Default_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            int numberOfCalls = 0;

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, async (el, ct) =>
            {
                await Task.Delay(500, ct).ConfigureAwait(false);
                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, maxBatchSize: 4, estimatedResultSize: 10);

            await foreach (var result in asyncEnumerable)
            {
                Assert.IsTrue((result > 0 && result < 11), "Expected a result between 1 and 10");
            }

            Assert.AreEqual(10, numberOfCalls);
        }

        [Test]
        public async Task ParallelAsync_Can_Process_IAsyncEnumerable_Streaming_Using_Default_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.AsAsyncEnumerable();

            int numberOfCalls = 0;

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, async (el, ct) =>
            {
                await Task.Delay(500, ct).ConfigureAwait(false);
                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, maxBatchSize: 4, estimatedResultSize: 10);

            await foreach (var result in asyncEnumerable)
            {
                Assert.IsTrue((result > 0 && result < 11), "Expected a result between 1 and 10");
            }

            Assert.AreEqual(10, numberOfCalls);
        }

        [Test]
        public async Task ParallelAsync_Can_Process_IEnumerable_Streaming_Using_Provided_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, async (el, ct) =>
            {
                await Task.Delay(500).ConfigureAwait(false);

                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, cancellationToken: cancellationToken, maxBatchSize: 1, estimatedResultSize: 10);

            var numberOfResults = 0;
            await foreach (var result in asyncEnumerable)
            {
                Assert.IsTrue((result > 0 && result < 11), "Expected a result between 1 and 10");
                numberOfResults++;
            }

            Assert.IsTrue(numberOfCalls < 10);
            Assert.IsTrue(numberOfResults <= numberOfCalls, $"Expected less than, or equal to, {numberOfCalls}, but got {numberOfResults}");

            cts.Dispose();
        }

        [Test]
        public async Task ParallelAsync_Can_Process_IAsyncEnumerable_Streaming_Using_Provided_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.AsAsyncEnumerable();

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            var asyncEnumerable = ParallelAsync.ForEachAsyncStream(input, async (el, ct) =>
            {
                await Task.Delay(500).ConfigureAwait(false);

                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, cancellationToken: cancellationToken, maxBatchSize: 1, estimatedResultSize: 10);

            var numberOfResults = 0;
            await foreach (var result in asyncEnumerable)
            {
                Assert.IsTrue((result > 0 && result < 11), "Expected a result between 1 and 10");
                numberOfResults++;
            }

            Assert.IsTrue(numberOfCalls < 10);
            Assert.IsTrue(numberOfResults <= numberOfCalls, $"Expected less than, or equal to, {numberOfCalls}, but got {numberOfResults}");

            cts.Dispose();
        }

        [Test]
        public void ParallelAsync_Throws_On_Invalid_Inputs()
        {
            var empty = new int[0];
            IEnumerable<int> nullEnumerable = null;

            var emptyAsync = new int[0].AsAsyncEnumerable();
            IAsyncEnumerable<int> nullAsyncEnumerable = null;

            var ex1 = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(nullEnumerable, (e) => Task.FromResult(e)))
                {
                }
            });
            var ex2 = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(nullEnumerable, (e, ct) => Task.FromResult(e)))
                {
                }
            });

            var ex3 = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(nullAsyncEnumerable, (e) => Task.FromResult(e)))
                {
                }
            });
            var ex4 = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(nullAsyncEnumerable, (e, ct) => Task.FromResult(e)))
                {
                }
            });

            var ex5 = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(empty, (Func<int, Task<int>>)null))
                {
                }
            });
            var ex6 = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(empty, (Func<int, CancellationToken, Task<int>>)null))
                {
                }
            });

            var ex7 = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(emptyAsync, (Func<int, Task<int>>)null))
                {
                }
            });
            var ex8 = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(emptyAsync, (Func<int, CancellationToken, Task<int>>)null))
                {
                }
            });

            var ex9 = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(empty, (e, ct) => Task.FromResult(e), -1))
                {
                }
            });
            var ex10 = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await foreach (var i in ParallelAsync.ForEachAsyncStream<int, int>(emptyAsync, (e, ct) => Task.FromResult(e), -1))
                {
                }
            });

            Assert.AreEqual("collection", ex1.ParamName);
            Assert.AreEqual("collection", ex2.ParamName);
            Assert.AreEqual("collection", ex3.ParamName);
            Assert.AreEqual("collection", ex4.ParamName);

            Assert.AreEqual("func", ex5.ParamName);
            Assert.AreEqual("func", ex6.ParamName);
            Assert.AreEqual("func", ex7.ParamName);
            Assert.AreEqual("func", ex8.ParamName);

            Assert.AreEqual("maxBatchSize", ex9.ParamName);
            Assert.AreEqual("maxBatchSize", ex10.ParamName);
        }
    }

    #endif //NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER

}
