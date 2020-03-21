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

        public static void Remove(this ICacher cacher, string key)
            => cacher.RemoveAsync(key).ExecuteSynchronously();

        public static async Task<TVal> FindOrCreateValueAsync<TVal>(this ICacher cacher, string key, Func<Task<TVal>> asyncCreator, TimeSpan? cacheEntryTimeout = null, bool forceCreate = false)
        {
            Requires.NonNull(asyncCreator, nameof(asyncCreator));

            var entry = await cacher.FindEntryOrCreateValueAsync(
                key,
                async k =>
                {
                    var val = await asyncCreator();
                    return new CacheCreationResult(val, new CacheEntryRetentionPolicy(cacheEntryTimeout));
                },
                forceCreate ? FindOrCreateEntrySettings.ForceCreateTrue : FindOrCreateEntrySettings.ForceCreateFalse
                );
            return entry.GetValue<TVal>();
        }

        public static TVal FindValue<TVal>(this ICacher cacher, string key, TVal missing = default(TVal))
            => cacher.FindValueAsync(key, missing).ExecuteSynchronously();

        public static async Task<TVal> FindValueAsync<TVal>(this ICacher cacher, string key, TVal missing=default(TVal))
        {
            var entry = await cacher.FindEntryAsync(key);
            return entry == null ? missing : entry.GetValue<TVal>();
        }

        public static Task<ICacheEntry> FindEntryAsync(this ICacher cacher, string key)
            => cacher.FindEntryOrCreateValueAsync(key);

        public static TVal FindOrCreateValue<TVal>(this ICacher cacher, string key, Func<TVal> creator, TimeSpan? cacheEntryTimeout = null, bool forceCreate = false)
            => cacher.FindOrCreateValueAsync<TVal>(key, () => Task.FromResult(creator()), cacheEntryTimeout, forceCreate).ExecuteSynchronously();

        public static TVal FindOrPrimeValues<TVal>(this ICacher cacher, string key, Func<IEnumerable<Tuple<string, TVal>>> primer, TimeSpan? cacheEntryTimeout = null)
        {
            var entry = cacher.FindEntryAsync(key).ExecuteSynchronously();
            if (entry != null && !entry.IsExpired) return entry.GetValue<TVal>();
            TVal ret = default;
            foreach (var t in primer())
            {
                cacher.FindOrCreateValue<TVal>(t.Item1, () => t.Item2, cacheEntryTimeout, true);
                if (t.Item1 == key)
                {
                    ret = t.Item2;
                }
            }
            return ret;
        }
    }
}
