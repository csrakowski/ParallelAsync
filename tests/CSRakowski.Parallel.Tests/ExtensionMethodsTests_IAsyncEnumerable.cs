﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRakowski.Parallel;
using NUnit.Framework;
using CSRakowski.Parallel.Extensions;
using System.Threading;
using CSRakowski.Parallel.Helpers;
using CSRakowski.Parallel.Tests.Helpers;
using CSRakowski.AsyncStreamsPreparations;

namespace CSRakowski.Parallel.Tests
{
    [TestFixture, Category("ParallelAsync IAsyncEnumerable Extension Methods Tests")]
    public class ExtensionMethodsTests_IAsyncEnumerable
    {
        [Test]
        public async Task ParallelAsync_Runs_With_Default_Settings()
        {
            var input = Enumerable.Range(1, 10).ToList().AsAsyncEnumerable();

            var parallelAsync = input.AsParallelAsync();

            Assert.IsNotNull(parallelAsync);

            var results = await parallelAsync.ForEachAsync((el) => Task.FromResult(el * 2)).ConfigureAwait(false);

            Assert.IsNotNull(results);

            var list = results as List<int>;

            Assert.IsNotNull(list);

            Assert.AreEqual(10, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * (1 + i);
                Assert.AreEqual(expected, list[i]);
            }
        }

        [Test]
        public async Task ParallelAsync_Runs_With_Default_Settings2()
        {
            var input = Enumerable.Range(1, 10).ToList().AsAsyncEnumerable();

            var parallelAsync = input.AsParallelAsync();

            Assert.IsNotNull(parallelAsync);

            var results = await parallelAsync.ForEachAsync((el, ct) => Task.FromResult(el * 2)).ConfigureAwait(false);

            Assert.IsNotNull(results);

            var list = results as List<int>;

            Assert.IsNotNull(list);

            Assert.AreEqual(10, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * (1 + i);
                Assert.AreEqual(expected, list[i]);
            }
        }

        [Test]
        public async Task ParallelAsync_Runs_With_Default_Settings3()
        {
            int sum = 0;
            int count = 0;

            var input = Enumerable.Range(1, 10).ToList().AsAsyncEnumerable();

            var parallelAsync = input.AsParallelAsync();

            Assert.IsNotNull(parallelAsync);

            await parallelAsync.ForEachAsync((el) => {
                Interlocked.Add(ref sum, el);
                Interlocked.Increment(ref count);

                return TaskHelper.CompletedTask;
            }).ConfigureAwait(false);

            Assert.AreEqual(55, sum);
            Assert.AreEqual(10, count);
        }

        [Test]
        public async Task ParallelAsync_Runs_With_Default_Settings4()
        {
            int sum = 0;
            int count = 0;

            var input = Enumerable.Range(1, 10).ToList().AsAsyncEnumerable();

            var parallelAsync = input.AsParallelAsync();

            Assert.IsNotNull(parallelAsync);

            await parallelAsync.ForEachAsync((el, ct) => {
                Interlocked.Add(ref sum, el);
                Interlocked.Increment(ref count);

                return TaskHelper.CompletedTask;
            }).ConfigureAwait(false);

            Assert.AreEqual(55, sum);
            Assert.AreEqual(10, count);
        }

        [Test]
        public async Task ParallelAsync_Supports_Full_Fluent_Usage()
        {
            var results =  await Enumerable
                                    .Range(1, 10)
                                    .AsAsyncEnumerable()
                                    .AsParallelAsync()
                                    .WithEstimatedResultSize(10)
                                    .WithMaxDegreeOfParallelism(2)
                                    .WithOutOfOrderProcessing(false)
                                    .ForEachAsync((el) => Task.FromResult(el * 2), CancellationToken.None)
                                    .ConfigureAwait(false);

            Assert.IsNotNull(results);

            var list = results as List<int>;

            Assert.IsNotNull(list);

            Assert.AreEqual(10, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * (1 + i);
                Assert.AreEqual(expected, list[i]);
            }
        }

        [Test]
        public void ParallelAsync_Handles_Invalid_Input_As_Expected()
        {
            IAsyncEnumerable<int> nullEnumerable = null;

            Assert.Throws<ArgumentNullException>(() => ParallelAsyncEx.AsParallelAsync<int>(nullEnumerable));

            var testCol = new List<int>().AsAsyncEnumerable().AsParallelAsync();

            Assert.Throws<ArgumentNullException>(() => ParallelAsyncEx.WithOutOfOrderProcessing<int>(null, true));

            Assert.Throws<ArgumentNullException>(() => ParallelAsyncEx.WithEstimatedResultSize<int>(null, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => testCol.WithEstimatedResultSize(-1));

            Assert.Throws<ArgumentNullException>(() => ParallelAsyncEx.WithMaxDegreeOfParallelism<int>(null, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => testCol.WithMaxDegreeOfParallelism(-1));
        }

        [Test]
        public void ParallelAsync_Handles_Double_Calls_Correctly()
        {
            var testCol = new List<int>().AsAsyncEnumerable().AsParallelAsync();

            var testCol2 = testCol.AsParallelAsync();

            Assert.AreSame(testCol, testCol2);
        }

        [Test]
        public void ParallelAsync_IParallelAsyncEnumerable_Throws_NotSupportedException_When_Casted_Into_IEnumerable()
        {
            var input = Enumerable.Range(1, 10).ToList().AsAsyncEnumerable();
            var testCol = input.AsParallelAsync();

            Assert.Throws<NotSupportedException>(() => testCol.GetEnumerator());

            IEnumerable enumerable = testCol;

            Assert.Throws<NotSupportedException>(() => enumerable.GetEnumerator());
        }
    }
}
