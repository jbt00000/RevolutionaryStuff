using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    public class BasicCacher : ICacher
    {
        private readonly IDictionary<string, ICacheEntry> EntriesByKey = new Dictionary<string, ICacheEntry>();

        public async Task<ICacheEntry> FindOrCreateEntryAsync(string key, Func<string, Task<ICacheEntry>> asyncCreator = null, bool forceCreate = false)
        {
            ICacheEntry e = null;
            if (forceCreate || !EntriesByKey.TryGetValue(key, out e) || e.IsExpired)
            {
                e = null;
                if (asyncCreator != null)
                {
                    e = await asyncCreator(key);
                }
                EntriesByKey[key] = e;
            }
            return e;
        }

        Task ICacher.RemoveAsync(string key)
        {
            EntriesByKey.Remove(key);
            return Task.CompletedTask;
        }
    }
}
