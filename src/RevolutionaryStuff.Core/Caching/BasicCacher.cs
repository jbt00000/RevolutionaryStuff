using System;
using System.Collections.Generic;

namespace RevolutionaryStuff.Core.Caching
{
    public class BasicCacher : ICacher
    {
        private readonly IDictionary<string, ICacheEntry> EntriesByKey = new Dictionary<string, ICacheEntry>();

        public CacheEntry<TVal> FindOrCreate<TVal>(string key, Func<string, CacheEntry<TVal>> creator, bool forceCreate, TimeSpan? timeout)
        {
            ICacheEntry e = null;
            if (forceCreate || !EntriesByKey.TryGetValue(key, out e) || e.IsExpired)
            {
                if (creator != null)
                {
                    e = creator(key);
                    EntriesByKey[key] = e;
                }
            }
            return e as CacheEntry<TVal>;
        }
    }
}
