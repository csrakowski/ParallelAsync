using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRakowski.Parallel;
using Xunit;
using CSRakowski.Parallel.Helpers;
using System.Threading;
using CSRakowski.Parallel.Tests.Helpers;

namespace CSRakowski.Parallel.Tests
{
    [Collection("ParallelAsync Helpers Tests")]
    public class HelpersTests
    {
        [Fact]
        public void ListHelper_Can_Determine_Sizes_Correctly()
        {
            var input = Enumerable.Range(1, 10).ToList();

            IList<int> list = input;
            ICollection<int> collectionT = input;
            IReadOnlyCollection<int> readOnlyCollection = new TestReadOnlyCollection<int>(10);
            var collection = new TestCollection<int>(10);

            IEnumerable<int> enumerable = Enumerable.Range(1, 10);
            IEnumerable<int> nullCollection = null;

            var listSize = ListHelpers.DetermineResultSize(list, -1);
            var readOnlyListSize = ListHelpers.DetermineResultSize(readOnlyCollection, -1);
            var collectionSize = ListHelpers.DetermineResultSize(collection, -1);
            var collectionTSize = ListHelpers.DetermineResultSize(collectionT, -1);

            var nullSize = ListHelpers.DetermineResultSize(nullCollection, -1);
            var enumerableSize = ListHelpers.DetermineResultSize(enumerable, -1);

            Assert.Equal(10, listSize);
            Assert.Equal(10, readOnlyListSize);
            Assert.Equal(10, collectionSize);
            Assert.Equal(10, collectionTSize);

            Assert.Equal(0, nullSize);

#if NET8_0_OR_GREATER
            // Due to .NET internal refactorings around the RangeIterator, the ListHelper now picks this up as an ICollection<T>, and we do actually get the actual size out of it.
            Assert.Equal(10, enumerableSize);
#else
            Assert.Equal(-1, enumerableSize);
#endif
        }

        [Fact]
        public void ListHelper_GetList_Handles_Negative_Numbers_Correctly()
        {
            var list1 = ListHelpers.GetList<int>(10);
            var list2 = ListHelpers.GetList<int>(0);
            var list3 = ListHelpers.GetList<int>(-1);

            Assert.Equal(10, list1.Capacity);
            Assert.Equal(0, list2.Capacity);
            Assert.Equal(0, list3.Capacity);
        }
    }
}
