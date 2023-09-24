using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSRakowski.Parallel;
using CSRakowski.Parallel.Tests.Helpers;

namespace CSRakowski.Parallel.Tests
{
    [Collection("ParallelAsync Base Tests")]
    public class ParallelAsyncTests
    {
        [Fact]
        public async Task ParallelAsync_Can_Batch_Basic_Work()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var results = await ParallelAsync.ForEachAsync(input, (el) => Task.FromResult(el * 2), maxBatchSize: 1, estimatedResultSize: input.Length);

            Assert.NotNull(results);

            var list = results as List<int>;

            Assert.NotNull(list);

            Assert.Equal(input.Length, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * input[i];
                Assert.Equal(expected, list[i]);
            }
        }

        [Fact]
        public async Task ParallelAsync_Can_Batch_Basic_Work_Void()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            await ParallelAsync.ForEachAsync(input, (el) =>
            {
                return TaskHelper.CompletedTask;
            }, maxBatchSize: 1)
;

            Assert.True(true);
        }

        [Fact]
        public async Task ParallelAsync_Can_Handle_Misaligned_Sizing()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cancellationToken = CancellationToken.None;

            var results = await ParallelAsync.ForEachAsync(input, (el) => Task.FromResult(el * 2), maxBatchSize: 4, estimatedResultSize: input.Length, cancellationToken: cancellationToken);

            Assert.NotNull(results);

            var list = results as List<int>;

            Assert.NotNull(list);

            Assert.Equal(input.Length, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * input[i];
                Assert.Equal(expected, list[i]);
            }
        }

        [Fact]
        public async Task ParallelAsync_Can_Handle_Misaligned_Sizing_Without_EstimatedSize()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cancellationToken = CancellationToken.None;

            var results = await ParallelAsync.ForEachAsync(input, (el) => Task.FromResult(el * 2), maxBatchSize: 4, cancellationToken: cancellationToken);

            Assert.NotNull(results);

            var list = results as List<int>;

            Assert.NotNull(list);

            Assert.Equal(input.Length, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * input[i];
                Assert.Equal(expected, list[i]);
            }
        }

        [Fact]
        public async Task ParallelAsync_Can_Handle_Misaligned_Sizing_Void()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cancellationToken = CancellationToken.None;

            await ParallelAsync.ForEachAsync(input, (el) =>
            {
                return TaskHelper.CompletedTask;
            }, maxBatchSize: 4, cancellationToken: cancellationToken)
;

            Assert.True(true);
        }

        [Fact]
        public async Task ParallelAsync_Can_Handle_Misaligned_Sizing_Void_Without_EstimatedSize()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cancellationToken = CancellationToken.None;

            await ParallelAsync.ForEachAsync(input, (el) =>
            {
                return TaskHelper.CompletedTask;
            }, maxBatchSize: 4, cancellationToken: cancellationToken)
;

            Assert.True(true);
        }

        [Fact]
        public async Task ParallelAsync_Can_Handle_Propagating_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            await ParallelAsync.ForEachAsync(input, async (el) =>
            {
                await Task.Delay(500);
                Interlocked.Increment(ref numberOfCalls);
            }, cancellationToken: cancellationToken, maxBatchSize: 4)
;

            Assert.True(numberOfCalls < 10);

            cts.Dispose();
        }

        [Fact]
        public async Task ParallelAsync_Can_Handle_Using_Default_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            int numberOfCalls = 0;

            await ParallelAsync.ForEachAsync(input, async (el, ct) =>
            {
                await Task.Delay(500, ct);
                Interlocked.Increment(ref numberOfCalls);
            });

            Assert.Equal(10, numberOfCalls);
        }

        [Fact]
        public async Task ParallelAsync_TaskT_Can_Handle_Propagating_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            var results = await ParallelAsync.ForEachAsync(input, async (el) =>
            {
                await Task.Delay(500);

                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, cancellationToken: cancellationToken, maxBatchSize: 4)
;

            Assert.True(numberOfCalls < 10);
            var numberOfResults = results.Count();
            Assert.True(numberOfResults <= numberOfCalls, $"Expected less than, or equal to, {numberOfCalls}, but got {numberOfResults}");

            cts.Dispose();
        }

        [Fact]
        public async Task ParallelAsync_TaskT_No_Batching()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            var results = await ParallelAsync.ForEachAsync(input, (el, ct) =>
            {
                Interlocked.Increment(ref numberOfCalls);

                return Task.FromResult(el);
            }, maxBatchSize: 1, cancellationToken: cancellationToken)
;

            Assert.Equal(10, numberOfCalls);
            Assert.Equal(numberOfCalls, results.Count());

            cts.Dispose();
        }

        [Fact]
        public async Task ParallelAsync_Throws_On_Invalid_Inputs()
        {
            var empty = new int[0];
            IEnumerable<int> nullEnumerable = null;

            var ex1 = await Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int>(nullEnumerable, (e) => TaskHelper.CompletedTask));
            var ex2 = await Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int>(nullEnumerable, (e, ct) => TaskHelper.CompletedTask));

            var ex3 = await Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int, int>(nullEnumerable, (e) => Task.FromResult(e)));
            var ex4 = await Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int, int>(nullEnumerable, (e, ct) => Task.FromResult(e)));

            var ex5 = await Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int>(empty, (Func<int, Task>)null));
            var ex6 = await Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int>(empty, (Func<int, CancellationToken, Task>)null));

            var ex7 = await Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int, int>(empty, (Func<int, Task<int>>)null));
            var ex8 = await Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int, int>(empty, (Func<int, CancellationToken, Task<int>>)null));

            var ex9 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ParallelAsync.ForEachAsync<int>(empty, (e) => TaskHelper.CompletedTask, -1));

            Assert.Equal("collection", ex1.ParamName);
            Assert.Equal("collection", ex2.ParamName);
            Assert.Equal("collection", ex3.ParamName);
            Assert.Equal("collection", ex4.ParamName);

            Assert.Equal("func", ex5.ParamName);
            Assert.Equal("func", ex6.ParamName);
            Assert.Equal("func", ex7.ParamName);
            Assert.Equal("func", ex8.ParamName);

            Assert.Equal("maxBatchSize", ex9.ParamName);
        }

        [Fact]
        public async Task ParallelAsync_Can_Batch_Basic_Work_Unordered()
        {
            const int numberOfElements = 100;
            var callCount = 0;
            var input = Enumerable.Range(1, numberOfElements).ToArray();

            var results = await ParallelAsync.ForEachAsync(input, (el) =>
            {
                var r = el + Interlocked.Increment(ref callCount);

                return Task.FromResult(r);
            }, maxBatchSize: 9, allowOutOfOrderProcessing: true, estimatedResultSize: input.Length)
;

            Assert.Equal(numberOfElements, callCount);
            Assert.Equal(numberOfElements, results.Count());
        }

        [Fact]
        public async Task ParallelAsync_Can_Batch_Basic_Work_Void_Unordered()
        {
            const int numberOfElements = 100;
            var callCount = 0;
            var input = Enumerable.Range(1, numberOfElements).ToArray();

            await ParallelAsync.ForEachAsync(input, (el) =>
            {
                var r = el + Interlocked.Increment(ref callCount);

                return TaskHelper.CompletedTask;
            }, maxBatchSize: 9, allowOutOfOrderProcessing: true)
;

            Assert.Equal(numberOfElements, callCount);
        }

        [Fact]
        public async Task ParallelAsync_Can_Handle_Propagating_CancellationTokens_Unordered()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            await ParallelAsync.ForEachAsync(input, async (el) =>
            {
                await Task.Delay(500);
                Interlocked.Increment(ref numberOfCalls);
            }, allowOutOfOrderProcessing: true, maxBatchSize: 4, cancellationToken: cancellationToken)
;

            Assert.True(numberOfCalls < 10);

            cts.Dispose();
        }

        [Fact]
        public async Task ParallelAsync_TaskT_Can_Handle_Propagating_CancellationTokens_Unordered()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            var results = await ParallelAsync.ForEachAsync(input, async (el) =>
            {
                await Task.Delay(500);

                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, allowOutOfOrderProcessing: true, maxBatchSize: 4, cancellationToken: cancellationToken)
;

            Assert.True(numberOfCalls < 10);
            var numberOfResults = results.Count();
            Assert.True(numberOfResults <= numberOfCalls, $"Expected less than, or equal to, {numberOfCalls}, but got {numberOfResults}");

            cts.Dispose();
        }
    }
}
