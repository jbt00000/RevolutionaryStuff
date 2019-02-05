using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    public class BasicCacher : ICacher
    {
        private readonly IDictionary<string, ICacheEntry> EntriesByKey = new Dictionary<string, ICacheEntry>();

        public async Task<ICacheEntry> FindOrCreateEntryAsync(string key, Func<string, Task<ICacheEntry>> asyncCreator = null, IFindOrCreateEntrySettings settings=null)
        {
            ICacheEntry e = null;

            settings = settings ?? FindOrCreateEntrySettings.Default;
            if (settings.ForceCreate || !EntriesByKey.TryGetValue(key, out e) || e.IsExpired)
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
