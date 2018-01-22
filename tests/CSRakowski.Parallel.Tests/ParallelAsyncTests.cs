using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSRakowski.Parallel;

namespace CSRakowski.Parallel.Tests
{
    /// <summary>
    /// As Task.CompletedTask is not available on 4.5, I slightly modified the code found here: http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs,66f1c3e3e272f591
    /// </summary>
    static class TaskHelper
    {
        /// <summary>
        /// A task that's already been completed successfully.
        /// </summary>
        private static Task s_completedTask;

        /// <summary>Gets a task that's already been completed successfully.</summary>
        /// <remarks>May not always return the same instance.</remarks>
        public static Task CompletedTask
        {
            get
            {
                var completedTask = s_completedTask;
                if (completedTask == null)
                {
#if NET45
                    s_completedTask = completedTask = Task.FromResult(true);
#else
                    s_completedTask = completedTask = Task.CompletedTask;
#endif
                }
                return completedTask;
            }
        }
    }

    [TestFixture, Category("ParallelAsync Base Tests")]
    public class ParallelAsyncTests
    {
        [Test]
        public async Task ParallelAsync_Can_Batch_Basic_Work()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var results = await ParallelAsync.ForEachAsync(input, (el) => Task.FromResult(el * 2), maxBatchSize: 1, estimatedResultSize: input.Length);

            Assert.IsNotNull(results);

            var list = results as List<int>;

            Assert.IsNotNull(list);

            Assert.AreEqual(input.Length, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * input[i];
                Assert.AreEqual(expected, list[i]);
            }
        }

        [Test]
        public async Task ParallelAsync_Can_Batch_Basic_Work_Void()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            await ParallelAsync.ForEachAsync(input, (el) =>
            {
                return TaskHelper.CompletedTask;
            }, maxBatchSize: 1);
        }

        [Test]
        public async Task ParallelAsync_Can_Handle_Misaligned_Sizing()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cancellationToken = CancellationToken.None;

            var results = await ParallelAsync.ForEachAsync(input, (el) => Task.FromResult(el * 2), maxBatchSize: 4, estimatedResultSize: input.Length, cancellationToken: cancellationToken);

            Assert.IsNotNull(results);

            var list = results as List<int>;

            Assert.IsNotNull(list);

            Assert.AreEqual(input.Length, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * input[i];
                Assert.AreEqual(expected, list[i]);
            }
        }

        [Test]
        public async Task ParallelAsync_Can_Handle_Misaligned_Sizing_Without_EstimatedSize()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cancellationToken = CancellationToken.None;

            var results = await ParallelAsync.ForEachAsync(input, (el) => Task.FromResult(el * 2), maxBatchSize: 4, cancellationToken: cancellationToken);

            Assert.IsNotNull(results);

            var list = results as List<int>;

            Assert.IsNotNull(list);

            Assert.AreEqual(input.Length, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * input[i];
                Assert.AreEqual(expected, list[i]);
            }
        }

        [Test]
        public async Task ParallelAsync_Can_Handle_Misaligned_Sizing_Void()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cancellationToken = CancellationToken.None;

            await ParallelAsync.ForEachAsync(input, (el) =>
            {
                return TaskHelper.CompletedTask;
            }, maxBatchSize: 4, cancellationToken: cancellationToken);
        }

        [Test]
        public async Task ParallelAsync_Can_Handle_Misaligned_Sizing_Void_Without_EstimatedSize()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cancellationToken = CancellationToken.None;

            await ParallelAsync.ForEachAsync(input, (el) =>
            {
                return TaskHelper.CompletedTask;
            }, maxBatchSize: 4, cancellationToken: cancellationToken);
        }

        [Test]
        public async Task ParallelAsync_Can_Handle_Propagating_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            await ParallelAsync.ForEachAsync(input, async (el, ct) =>
            {
                await Task.Delay(500);
                Interlocked.Increment(ref numberOfCalls);
            }, cancellationToken: cancellationToken);

            Assert.IsTrue(numberOfCalls < 10);

            cts.Dispose();
        }

        [Test]
        public async Task ParallelAsync_Can_Handle_Using_Default_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            int numberOfCalls = 0;

            await ParallelAsync.ForEachAsync(input, async (el, ct) =>
            {
                await Task.Delay(500, ct);
                Interlocked.Increment(ref numberOfCalls);
            });

            Assert.AreEqual(10, numberOfCalls);
        }

        [Test]
        public async Task ParallelAsync_TaskT_Can_Handle_Propagating_CancellationTokens()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            var results = await ParallelAsync.ForEachAsync(input, async (el, ct) =>
            {
                await Task.Delay(500);

                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, cancellationToken: cancellationToken);

            Assert.IsTrue(numberOfCalls < 10);
            var numberOfResults = results.Count();
            Assert.IsTrue(numberOfResults <= numberOfCalls, $"Expected less than, or equal to, {numberOfCalls}, but got {numberOfResults}");

            cts.Dispose();
        }

        [Test]
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
            }, maxBatchSize: 1, cancellationToken: cancellationToken);

            Assert.AreEqual(10, numberOfCalls);
            Assert.AreEqual(numberOfCalls, results.Count());

            cts.Dispose();
        }

        [Test]
        public void ParallelAsync_Throws_On_Invalid_Inputs()
        {
            var empty = new int[0];
            IEnumerable<int> nullEnumerable = null;

            var ex1 = Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int>(nullEnumerable, (e) => TaskHelper.CompletedTask));
            var ex2 = Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int>(nullEnumerable, (e, ct) => TaskHelper.CompletedTask));

            var ex3 = Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int, int>(nullEnumerable, (e) => Task.FromResult(e)));
            var ex4 = Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int, int>(nullEnumerable, (e, ct) => Task.FromResult(e)));

            var ex5 = Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int>(empty, (Func<int, Task>)null));
            var ex6 = Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int>(empty, (Func<int, CancellationToken, Task>)null));

            var ex7 = Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int, int>(empty, (Func<int, Task<int>>)null));
            var ex8 = Assert.ThrowsAsync<ArgumentNullException>(() => ParallelAsync.ForEachAsync<int, int>(empty, (Func<int, CancellationToken, Task<int>>)null));

            var ex9 = Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ParallelAsync.ForEachAsync<int>(empty, (e) => TaskHelper.CompletedTask, -1));

            Assert.AreEqual("collection", ex1.ParamName);
            Assert.AreEqual("collection", ex2.ParamName);
            Assert.AreEqual("collection", ex3.ParamName);
            Assert.AreEqual("collection", ex4.ParamName);

            Assert.AreEqual("func", ex5.ParamName);
            Assert.AreEqual("func", ex6.ParamName);
            Assert.AreEqual("func", ex7.ParamName);
            Assert.AreEqual("func", ex8.ParamName);

            Assert.AreEqual("maxBatchSize", ex9.ParamName);
        }

        [Test]
        public async Task ParallelAsync_Can_Batch_Basic_Work_Unordered()
        {
            const int numberOfElements = 100;
            var callCount = 0;
            var input = Enumerable.Range(1, numberOfElements).ToArray();

            var results = await ParallelAsync.ForEachAsync(input, (el) =>
            {
                var r = el + Interlocked.Increment(ref callCount);

                return Task.FromResult(r);
            }, maxBatchSize: 9, allowOutOfOrderProcessing: true, estimatedResultSize: input.Length);

            Assert.AreEqual(numberOfElements, callCount);
            Assert.AreEqual(numberOfElements, results.Count());
        }

        [Test]
        public async Task ParallelAsync_Can_Batch_Basic_Work_Void_Unordered()
        {
            const int numberOfElements = 100;
            var callCount = 0;
            var input = Enumerable.Range(1, numberOfElements).ToArray();

            await ParallelAsync.ForEachAsync(input, (el) =>
            {
                var r = el + Interlocked.Increment(ref callCount);

                return TaskHelper.CompletedTask;
            }, maxBatchSize: 9, allowOutOfOrderProcessing: true);

            Assert.AreEqual(numberOfElements, callCount);
        }

        [Test]
        public async Task ParallelAsync_Can_Handle_Propagating_CancellationTokens_Unordered()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            await ParallelAsync.ForEachAsync(input, async (el, ct) =>
            {
                await Task.Delay(500);
                Interlocked.Increment(ref numberOfCalls);
            }, allowOutOfOrderProcessing: true, cancellationToken: cancellationToken);

            Assert.IsTrue(numberOfCalls < 10);

            cts.Dispose();
        }

        [Test]
        public async Task ParallelAsync_TaskT_Can_Handle_Propagating_CancellationTokens_Unordered()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            int numberOfCalls = 0;

            cts.CancelAfter(250);

            var results = await ParallelAsync.ForEachAsync(input, async (el, ct) =>
            {
                await Task.Delay(500);

                Interlocked.Increment(ref numberOfCalls);
                return el;
            }, allowOutOfOrderProcessing: true, cancellationToken: cancellationToken);

            Assert.IsTrue(numberOfCalls < 10);
            var numberOfResults = results.Count();
            Assert.IsTrue(numberOfResults <= numberOfCalls, $"Expected less than, or equal to, {numberOfCalls}, but got {numberOfResults}");

            cts.Dispose();
        }
    }
}
