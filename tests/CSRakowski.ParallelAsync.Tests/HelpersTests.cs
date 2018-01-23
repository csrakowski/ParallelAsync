using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRakowski.Parallel;
using NUnit.Framework;
using CSRakowski.Parallel.Helpers;
using System.Threading;

namespace CSRakowski.Parallel.Tests
{
    [TestFixture, Category("ParallelAsync Helpers Tests")]
    public class HelpersTests
    {
        private class TestCollection<T> : IEnumerable<T>, ICollection
        {
            private readonly int _size;

            public TestCollection(int size)
            {
                _size = size;
            }

            #region ICollection

            public int Count => _size;

            public Object SyncRoot => this;
            public bool IsSynchronized => true;

            public void CopyTo(Array array, int index) { }

            IEnumerator IEnumerable.GetEnumerator() => null;

            public IEnumerator<T> GetEnumerator() => null;

            #endregion ICollection

        }

        private class TestReadOnlyCollection<T> : IReadOnlyCollection<T>
        {
            private readonly int _size;

            public TestReadOnlyCollection(int size)
            {
                _size = size;
            }

            #region IReadOnlyCollection

            public int Count => _size;

            IEnumerator IEnumerable.GetEnumerator() => null;

            public IEnumerator<T> GetEnumerator() => null;

            #endregion IReadOnlyCollection
        }

        [Test]
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

            Assert.AreEqual(10, listSize, "Could not get correct size for the IList<T>");
            Assert.AreEqual(10, readOnlyListSize, "Could not get correct size for the IReadOnlyCollection<T>");
            Assert.AreEqual(10, collectionSize, "Could not get correct size for the ICollection");
            Assert.AreEqual(10, collectionTSize, "Could not get correct size for the ICollection<T>");

            Assert.AreEqual(0, nullSize, "Could not get correct size for the null input");
            Assert.AreEqual(-1, enumerableSize, "Could not get correct size for the IEnumerable<T>");
        }

        [Test]
        public void ListHelper_GetList_Handles_Negative_Numbers_Correctly()
        {
            var list1 = ListHelpers.GetList<int>(10);
            var list2 = ListHelpers.GetList<int>(0);
            var list3 = ListHelpers.GetList<int>(-1);

            Assert.AreEqual(10, list1.Capacity);
            Assert.AreEqual(0, list2.Capacity);
            Assert.AreEqual(0, list3.Capacity);
        }
    }
}
