using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RevolutionaryStuff.Core.Caching
{
    /// <summary>
    /// This is an implementation of ICache that uses a random replacement policy
    /// </summary>
    public class RandomCache<K, D> : ICache<K, D>
    {
        /// <summary>
        /// The storage for the cached data.  DataByKey
        /// </summary>
        protected readonly IDictionary<K, D> ht;

        /// <summary>
        /// The maximum number of items this cache can hold
        /// </summary>
        public readonly int MaxItems;

        protected uint Hits;
        protected uint Requests;

        #region Constructors

        /// <summary>
        /// Construct a new Cache
        /// </summary>
        public RandomCache()
            : this(512, null)
        {
        }

        /// <summary>
        /// Construct a new Cache
        /// </summary>
        /// <param name="maxItems">The maximum number of items the cache should hold</param>
        public RandomCache(int maxItems)
            : this(maxItems, null)
        {
        }

        /// <summary>
        /// Construct a new Cache
        /// </summary>
        /// <param name="maxItems">The maximum number of items the cache should hold</param>
        /// <param name="dict">The dictionary to use [opt]</param>
        public RandomCache(int maxItems, IDictionary<K, D> dict)
        {
            ht = dict;
            if (ht == null)
            {
                if (maxItems < 1 || maxItems == int.MaxValue)
                {
                    ht = new Dictionary<K, D>();
                }
                else
                {
                    ht = new Dictionary<K, D>(maxItems);
                }
            }
            MaxItems = maxItems < 1 ? int.MaxValue : maxItems;
        }

        #endregion

        protected uint Misses
        {
            [DebuggerStepThrough]
            get { return Requests - Hits; }
        }

        #region ICache<K,D> Members

        /// <summary>
        /// Add a new item to the cache.
        /// </summary>
        /// <remarks>Existing values of the same key will be replaced</remarks>
        /// <param name="key">The key</param>
        /// <param name="data">The data</param>
        public void Add(K key, D data)
        {
            if (!ht.ContainsKey(key) && ht.Count >= MaxItems)
            {
                int r = ht.Count - MaxItems + 1;
                Debug.Assert(r == 1);
                var deadKey = (K)CollectionHelpers.GetItem(ht.Keys, Stuff.Random.Next(ht.Count), null);
                ht.Remove(deadKey);
            }
            Debug.Assert(ht.Count < (long)MaxItems + 10);
            ht[key] = data;
        }

        /// <summary>
        /// Remove an entry (if it exists) from the cache
        /// </summary>
        /// <param name="key">The key</param>
        public void Remove(K key)
        {
            ht.Remove(key);
        }

        /// <summary>
        /// Get's an entry from the cache
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="data">If the key was in the cache, the data, else null</param>
        /// <returns>True if the data was in the cache, else false</returns>
        public bool Find(K key, out D data)
        {
            ++Requests;
            data = default(D);
            try
            {
                if (!ht.ContainsKey(key)) return false;
                data = ht[key];
                ++Hits;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Does the item exist in the cache?
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>True if the item exits, else false</returns>
        public bool Exists(K key)
        {
            return ht.ContainsKey(key);
        }

        /// <summary>
        /// Flush all entries from the cache
        /// </summary>
        public void Flush()
        {
            ht.Clear();
        }

        public int Count
        {
            get { return ht.Count; }
        }

        #endregion

        public override string ToString()
        {
            return string.Format("RandomCache cnt={0} hits={1} misses={2}", Count, Hits, Misses);
        }
    }
}
