using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    public static class CacherHelpers
    {
        public static TVal GetValue<TVal>(this ICacheEntry cacheEntry)
            => (TVal)cacheEntry.Value;

        public static ICacher CreateScope(this ICacher inner, params object[] keyParts)
            => new ScopedCacher(inner, keyParts);

        public static async Task<TVal> FindOrCreateValueAsync<TVal>(this ICacher cacher, string key, Func<Task<TVal>> asyncCreator, bool forceCreate = false, TimeSpan ? cacheEntryTimeout = null)
        {
            var entry = await cacher.FindOrCreateEntryAsync(
                key, 
                async k => 
                {
                    var val = await asyncCreator();
                    return new CacheEntry<TVal>(val, cacheEntryTimeout);
                }, 
                forceCreate);
            return (TVal) entry.Value;
        }

        public static ICacheEntry FindOrCreateEntry(this ICacher cacher, string key, Func<string, ICacheEntry> creator = null, bool forceCreate = false)
            => cacher.FindOrCreateEntryAsync(key, k => Task.FromResult(creator(k)), forceCreate).ExecuteSynchronously();

        public static TVal FindOrCreateValue<TVal>(this ICacher cacher, string key, Func<TVal> creator, bool forceCreate = false, TimeSpan? cacheEntryTimeout = null)
            => cacher.FindOrCreateValueAsync(key, () => Task.FromResult(creator()), forceCreate, cacheEntryTimeout).ExecuteSynchronously();

        public static TVal FindOrPrimeValues<TVal>(this ICacher cacher, string key, Func<IEnumerable<Tuple<string, TVal>>> primer, TimeSpan? cacheEntryTimeout = null)
        {
            var ret = cacher.FindOrCreateValue<TVal>(key, null);
            if (ret == null)
            {
                foreach (var t in primer())
                {
                    cacher.FindOrCreateValue(t.Item1, () => t.Item2, true, cacheEntryTimeout);
                    if (t.Item1 == key)
                    {
                        ret = t.Item2;
                    }
                }
            }
            return ret;
        }
    }
}
