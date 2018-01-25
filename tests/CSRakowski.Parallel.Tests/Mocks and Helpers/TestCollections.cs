using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Tests.Helpers
{
    internal class TestCollection<T> : IEnumerable<T>, ICollection
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

    internal class TestReadOnlyCollection<T> : IReadOnlyCollection<T>
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
}
