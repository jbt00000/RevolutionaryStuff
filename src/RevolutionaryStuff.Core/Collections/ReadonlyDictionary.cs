using System.Collections.Generic;

namespace RevolutionaryStuff.Core.Collections
{
    public class ReadonlyDictionary<K, V> : IDictionary<K, V>
    {
        private readonly IDictionary<K, V> Inner;

        public ReadonlyDictionary(IDictionary<K, V> inner)
        {
            Requires.NonNull("inner", nameof(inner));
            Inner = inner;
        }

        #region IDictionary<K,V> Members

        void IDictionary<K, V>.Add(K key, V value)
        {
            throw new ReadOnlyException();
        }

        bool IDictionary<K, V>.ContainsKey(K key)
        {
            return Inner.ContainsKey(key);
        }

        ICollection<K> IDictionary<K, V>.Keys
        {
            get { return Inner.Keys; }
        }

        bool IDictionary<K, V>.Remove(K key)
        {
            throw new ReadOnlyException();
        }

        bool IDictionary<K, V>.TryGetValue(K key, out V value)
        {
            return Inner.TryGetValue(key, out value);
        }

        ICollection<V> IDictionary<K, V>.Values
        {
            get { return Inner.Values; }
        }

        V IDictionary<K, V>.this[K key]
        {
            get
            {
                return Inner[key];
            }
            set
            {
                throw new ReadOnlyException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item)
        {
            throw new ReadOnlyException();
        }

        void ICollection<KeyValuePair<K, V>>.Clear()
        {
            throw new ReadOnlyException();
        }

        bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> item)
        {
            return Inner.Contains(item);
        }

        void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            Inner.CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<K, V>>.Count
        {
            get { return Inner.Count; }
        }

        bool ICollection<KeyValuePair<K, V>>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item)
        {
            throw new ReadOnlyException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
        {
            return Inner.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)Inner).GetEnumerator();
        }

        #endregion
    }
}
