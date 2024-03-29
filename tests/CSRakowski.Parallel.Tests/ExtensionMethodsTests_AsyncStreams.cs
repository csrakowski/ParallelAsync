﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRakowski.Parallel;
using Xunit;
using CSRakowski.Parallel.Extensions;
using System.Threading;
using CSRakowski.Parallel.Helpers;
using CSRakowski.Parallel.Tests.Helpers;
using CSRakowski.AsyncStreamsPreparations;

namespace CSRakowski.Parallel.Tests
{
    #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER

    [Collection("ParallelAsync AsyncStreams Extension Methods Tests")]
    public class ExtensionMethodsTests_AsyncStreams
    {
        [Fact]
        public async Task ParallelAsync_Runs_With_Default_Settings()
        {
            var input = Enumerable.Range(1, 10).ToList().AsAsyncEnumerable();

            var parallelAsync = input.AsParallelAsync();

            Assert.NotNull(parallelAsync);

            var list = new List<int>();

            await foreach (var item in parallelAsync.ForEachAsyncStream((el) => Task.FromResult(el * 2)))
            {
                list.Add(item);
            }

            Assert.Equal(10, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * (1 + i);
                Assert.Equal(expected, list[i]);
            }
        }

        [Fact]
        public async Task ParallelAsync_Runs_With_Default_Settings2()
        {
            var input = Enumerable.Range(1, 10).ToList().AsAsyncEnumerable();

            var parallelAsync = input.AsParallelAsync();

            Assert.NotNull(parallelAsync);

            var list = new List<int>();

            await foreach (var item in parallelAsync.ForEachAsyncStream((el, ct) => Task.FromResult(el * 2)))
            {
                list.Add(item);
            }

            Assert.Equal(10, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * (1 + i);
                Assert.Equal(expected, list[i]);
            }
        }

        [Fact]
        public async Task ParallelAsync_Supports_Full_Fluent_Usage()
        {
            var asyncEnumerable = Enumerable
                                .Range(1, 10)
                                .AsAsyncEnumerable()
                                .AsParallelAsync()
                                .WithEstimatedResultSize(10)
                                .WithMaxDegreeOfParallelism(2)
                                .WithOutOfOrderProcessing(false)
                                .ForEachAsyncStream((el) => Task.FromResult(el * 2), CancellationToken.None);

            Assert.NotNull(asyncEnumerable);

            var list = new List<int>();

            await foreach (var item in asyncEnumerable)
            {
                list.Add(item);
            }

            Assert.Equal(10, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 2 * (1 + i);
                Assert.Equal(expected, list[i]);
            }
        }

        [Fact]
        public async Task ParallelAsync_Can_Chain_Together_AsyncStreams()
        {
            var input = Enumerable.Range(1, 40).ToList().AsAsyncEnumerable();

            var parallelAsync = input.AsParallelAsync();

            Assert.NotNull(parallelAsync);

            var list = new List<int>();

            IAsyncEnumerable<int> intermediateResult = parallelAsync.ForEachAsyncStream((el, ct) => Task.FromResult(el * 2));

            var intermediateParallelAsync = intermediateResult.AsParallelAsync();

            await foreach (var item in intermediateParallelAsync.ForEachAsyncStream((el, ct) => Task.FromResult(el * 2)))
            {
                list.Add(item);
            }

            Assert.Equal(40, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var expected = 4 * (1 + i);
                Assert.Equal(expected, list[i]);
            }
        }
    }

    #endif //NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER

}
