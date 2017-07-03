using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core.Collections
{
    public class MultipleValueDictionary<K, V> : BaseModifyable, IEnumerable<KeyValuePair<K, ICollection<V>>>
    {
        private readonly V[] NoValues = new V[0];
        private readonly Dictionary<K, ICollection<V>> ValuesByKey;
        private readonly Func<ICollection<V>> CollectionCreator;

        public MultipleValueDictionary(IEqualityComparer<K> comparer=null, Func<ICollection<V>> collectionCreator=null)
        {
            CollectionCreator = collectionCreator;
            if (comparer == null)
            {
                ValuesByKey = new Dictionary<K, ICollection<V>>();
            }
            else
            {
                ValuesByKey = new Dictionary<K, ICollection<V>>(comparer);
            }
        }

        public override string ToString()
        {
            return string.Format("MultipleValueDictionary keyCount={1} itemCount={2}", this.GetType(), this.ValuesByKey.Count, this.AtomEnumerable.Count());
        }

        public IDictionary<K, IList<V>> ToDictionaryOfCollection() => ToDictionary<IList<V>>(vals => new List<V>(vals));

        public IDictionary<K, VPrime> ToDictionary<VPrime>(Func<ICollection<V>, VPrime> elementsMapper)
        {
            Requires.NonNull(elementsMapper, nameof(elementsMapper));

            var d = new Dictionary<K, VPrime>(this.Count);
            foreach (var key in this.Keys)
            {
                d[key] = elementsMapper(this[key]);
            }
            return d;
        }

        public void Set(K k, V v)
        {
            Set(k, new[] { v });
        }

        public void Set(K k, IEnumerable<V> vals)
        {
            if (ContainsKey(k))
            {
                Remove(k);
            }
            AddRange(k, vals);
        }

        protected override void OnMakeReadonly()
        {
            throw new NotImplementedException();
            /*
            foreach (var key in ValuesByKey.Keys.ToList())
            {
                ValuesByKey[key] = ValuesByKey[key].AsReadOnly();
            }
            base.OnMakeReadonly();
            */
        }

        public void Remove(K key)
        {
            CheckCanModify();
            ValuesByKey.Remove(key);
        }

        public void Remove(K key, V val)
        {
            CheckCanModify();
            ICollection<V> col;
            if (ValuesByKey.TryGetValue(key, out col))
            {
                if (col.Count == 1)
                {
                    ValuesByKey.Remove(key);
                }
                else
                {
                    col.Remove(val);
                }
            }
        }

        /// <summary>
        /// Gets the number of keys in MultipleValueDictionary<K,V>
        /// </summary>
        public int Count
        {
            [DebuggerStepThrough]
            get { return ValuesByKey.Count; }
        }

        public ICollection<V> this[K k]
        {
            get
            {
                ICollection<V> c;
                if (ValuesByKey.TryGetValue(k, out c))
                {
                    return c;
                }
                return NoValues;
            }
            set
            {
                CheckCanModify();
                ValuesByKey[k] = value ?? NoValues;
            }
        }

        public IEnumerable<IGrouping<K, V>> Groupings
        {
            get
            {
                foreach (var k in Keys)
                {
                    yield return new Grouping<K, V>(k, this[k]);
                }
            }
        }

        public IEnumerable<K> Keys
        {
            [DebuggerStepThrough]
            get { return ValuesByKey.Keys; }
        }

        public void Add(IEnumerable<KeyValuePair<K, ICollection<V>>> other)
        {
            foreach (var kvp in other)
            {
                AddRange(kvp.Key, kvp.Value);
            }
        }

        public void AddUnique(K k, V v)
        {
            Add(k, v, true);
        }

        public void Add(K k, V v)
        {
            Add(k, v, false);
        }

        private void Add(K k, V v, bool unique)
        {
            CheckCanModify();

            ICollection<V> c;
            if (ValuesByKey.TryGetValue(k, out c))
            {
                if (c is V[])
                {
                    if (CollectionCreator == null)
                    {
                        c = new List<V>(c);
                    }
                    else
                    {
                        c = CollectionCreator();
                    }
                    ValuesByKey[k] = c;
                }
                if (!unique || !c.Contains(v))
                {
                    c.Add(v);
                }
            }
            else
            {
                ValuesByKey[k] = new[] { v };
            }
        }

        public void AddRange(K k, IEnumerable<V> vs)
        {
            if (vs == null) return;
            foreach (V v in vs)
            {
                Add(k, v);
            }
        }

        /// <summary>
        /// Removes all of the elements from MultipleValueDictionary<K,V>
        /// </summary>
        public void Clear()
        {
            CheckCanModify();
            ValuesByKey.Clear();
        }

        public bool Contains(K key, V val)
        {
            ICollection<V> col;
            if (ValuesByKey.TryGetValue(key, out col))
            {
                return col.Contains(val);
            }
            return false;
        }

        public bool ContainsKey(K key)
        {
            return ValuesByKey.ContainsKey(key);
        }

        #region IEnumerable<KeyValuePair<K,ICollection<V>>> Members

        IEnumerator<KeyValuePair<K, ICollection<V>>> IEnumerable<KeyValuePair<K, ICollection<V>>>.GetEnumerator()
        {
            foreach (var k in Keys)
            {
                yield return new KeyValuePair<K, ICollection<V>>(k, this[k]);
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var k in Keys)
            {
                yield return new KeyValuePair<K, ICollection<V>>(k, this[k]);
            }
        }

        #endregion

        public IEnumerable<KeyValuePair<K, V>> AtomEnumerable
        {
            get
            {
                foreach (var k in Keys)
                {
                    foreach (var v in this[k])
                    {
                        yield return new KeyValuePair<K, V>(k, v);
                    }
                }
            }
        }
    }
}
