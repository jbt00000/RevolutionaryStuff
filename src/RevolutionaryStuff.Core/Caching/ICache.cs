using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core.Caching
{
    /// <summary>
    /// Base interface for caching classes
    /// </summary>
    public interface ICache<K, D> : IFlushable
    {
        /// <summary>
        /// The number of items in the cache
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Add a new item to the cache.
        /// </summary>
        /// <remarks>Existing values of the same key will be replaced</remarks>
        /// <param name="key">The key</param>
        /// <param name="data">The data</param>
        void Add(K key, D data);

        /// <summary>
        /// Remove an entry (if it exists) from the cache
        /// </summary>
        /// <param name="key">The key</param>
        void Remove(K key);

        /// <summary>
        /// Get's an entry from the cache
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="data">If the key was in the cache, the data, else null</param>
        /// <returns>True if the data was in the cache, else false</returns>
        bool Find(K key, out D data);

        /// <summary>
        /// Does the item exist in the cache?
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>True if the item exits, else false</returns>
        bool Exists(K key);
    }
}