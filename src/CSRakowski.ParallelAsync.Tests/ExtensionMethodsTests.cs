using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRakowski.Parallel;
using NUnit.Framework;
using CSRakowski.Parallel.Extensions;
using System.Threading;

namespace CSRakowski.Parallel.Tests
{
    [TestFixture, Category("ParallelAsync Extension Methods Tests")]
    public class ExtensionMethodsTests
    {
        [Test]
        public async Task ParallelAsync_Runs_With_Default_Settings()
        {
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var parallelAsync = input.AsParallelAsync();

            Assert.IsNotNull(parallelAsync);

            var results = await parallelAsync.ForEachAsync((el) => Task.FromResult(el * 2));

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
    }
}
