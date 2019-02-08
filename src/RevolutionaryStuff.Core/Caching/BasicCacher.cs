using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    public class BasicCacher : BaseCacher
    {
        //This is internal to support unit testing
        internal readonly IDictionary<string, ICacheEntry> CacheEntryByCacheKey = new Dictionary<string, ICacheEntry>();

        protected readonly int MaxEntries;

        public BasicCacher(int? maxEntries=null)
        {
            MaxEntries = maxEntries.GetValueOrDefault(int.MaxValue);
        }

        protected override Task<ICacheEntry> OnFindEntryAsync(string key)
            => Task.FromResult(CacheEntryByCacheKey.FindOrDefault(key));

        protected override Task OnWriteEntryAsync(string key, ICacheEntry entry)
        {
            if (CacheEntryByCacheKey.Count >= MaxEntries)
            {
                var keys = CacheEntryByCacheKey.Keys.ToList();
                keys.ShuffleList();
                CacheEntryByCacheKey.Remove(keys[0]);
            }
            CacheEntryByCacheKey[key] = entry;
            return Task.CompletedTask;
        }

        protected override Task OnRemoveAsync(string key)
        {
            CacheEntryByCacheKey.Remove(key);
            return Task.CompletedTask;
        }
    }
}
