using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRakowski.Parallel;
using NUnit.Framework;
using CSRakowski.Parallel.Extensions;
using System.Threading;
using CSRakowski.Parallel.Tests.Helpers;

namespace CSRakowski.Parallel.Tests
{
    [TestFixture, Category("ParallelAsync Extension Methods Tests")]
    public class ExtensionMethodsTests
    {
        [Test]
        public async Task ParallelAsync_Runs_With_Default_Settings()
        {
            var input = Enumerable.Range(1, 10).ToList();

            var parallelAsync = input.AsParallelAsync();

            Assert.IsNotNull(parallelAsync);

            var results = await parallelAsync.ForEachAsync((el) => Task.FromResult(el * 2));

            Assert.IsNotNull(results);

            var list = results as List<int>;

            Assert.IsNotNull(list);

            Assert.AreEqual(input.Count, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * input[i];
                Assert.AreEqual(expected, list[i]);
            }
        }

        [Test]
        public async Task ParallelAsync_Runs_With_Default_Settings2()
        {
            var input = Enumerable.Range(1, 10).ToList();

            var parallelAsync = input.AsParallelAsync();

            Assert.IsNotNull(parallelAsync);

            var results = await parallelAsync.ForEachAsync((el, ct) => Task.FromResult(el * 2));

            Assert.IsNotNull(results);

            var list = results as List<int>;

            Assert.IsNotNull(list);

            Assert.AreEqual(input.Count, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * input[i];
                Assert.AreEqual(expected, list[i]);
            }
        }

        [Test]
        public async Task ParallelAsync_Runs_With_Default_Settings3()
        {
            int sum = 0;
            int count = 0;

            var input = Enumerable.Range(1, 10).ToList();

            var parallelAsync = input.AsParallelAsync();

            Assert.IsNotNull(parallelAsync);

            await parallelAsync.ForEachAsync((el) => {
                Interlocked.Add(ref sum, el);
                Interlocked.Increment(ref count);

                return TaskHelper.CompletedTask;
            });

            Assert.AreEqual(55, sum);
            Assert.AreEqual(10, count);
        }

        [Test]
        public async Task ParallelAsync_Runs_With_Default_Settings4()
        {
            int sum = 0;
            int count = 0;

            var input = Enumerable.Range(1, 10).ToList();

            var parallelAsync = input.AsParallelAsync();

            Assert.IsNotNull(parallelAsync);

            await parallelAsync.ForEachAsync((el, ct) => {
                Interlocked.Add(ref sum, el);
                Interlocked.Increment(ref count);

                return TaskHelper.CompletedTask;
            });

            Assert.AreEqual(55, sum);
            Assert.AreEqual(10, count);
        }

        [Test]
        public async Task ParallelAsync_Supports_Full_Fluent_Usage()
        {
            var results =  await Enumerable
                                    .Range(1, 10)
                                    .AsParallelAsync()
                                    .WithEstimatedResultSize(10)
                                    .WithMaxDegreeOfParallelism(2)
                                    .WithOutOfOrderProcessing(false)
                                    .ForEachAsync((el) => Task.FromResult(el * 2), CancellationToken.None);

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
            IEnumerable<int> nullEnumerable = null;

            Assert.Throws<ArgumentNullException>(() => ParallelAsyncEx.AsParallelAsync<int>(nullEnumerable));

            var testCol = new List<int>().AsParallelAsync();

            Assert.Throws<ArgumentNullException>(() => ParallelAsyncEx.WithOutOfOrderProcessing<int>(null, true));

            Assert.Throws<ArgumentNullException>(() => ParallelAsyncEx.WithEstimatedResultSize<int>(null, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => testCol.WithEstimatedResultSize(-1));

            Assert.Throws<ArgumentNullException>(() => ParallelAsyncEx.WithMaxDegreeOfParallelism<int>(null, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => testCol.WithMaxDegreeOfParallelism(-1));
        }

        [Test]
        public void ParallelAsync_Handles_Double_Calls_Correctly()
        {
            var testCol = new List<int>().AsParallelAsync();

            var testCol2 = testCol.AsParallelAsync();

            Assert.AreSame(testCol, testCol2);
        }

        [Test]
        public void ParallelAsync_IParallelAsyncEnumerable_Can_Still_Be_Casted_To_IEnumerable_Correctly()
        {
            var input = Enumerable.Range(1, 10).ToList();
            var testCol = input.AsParallelAsync();

            int count = 0;
            int sum = 0;

            foreach (var item in testCol)
            {
                Interlocked.Add(ref sum, item);
                Interlocked.Increment(ref count);
            }

            Assert.AreEqual(55, sum);
            Assert.AreEqual(10, count);


            IEnumerable enumerable = testCol;

            int count2 = 0;

            foreach (var item in enumerable)
            {
                Interlocked.Increment(ref count2);
            }

            Assert.AreEqual(10, count2);
        }
    }
}
