namespace RevolutionaryStuff.Core.Caching;

public static class CacherHelpers
{
    extension(ICacheEntry source)
    {
        public TVal GetValue<TVal>()
                => (TVal)source.Value;
    }

    extension(ICacher source)
    {
        public ICacher CreateScope(params object[] keyParts)
                => new ScopedCacher(source, keyParts);
        public void Remove(string key)
                => source.RemoveAsync(key).ExecuteSynchronously();
        public async Task<TVal> FindOrCreateValueAsync<TVal>(string key, Func<Task<TVal>> asyncCreator, TimeSpan? cacheEntryTimeout = null, bool forceCreate = false)
        {
            ArgumentNullException.ThrowIfNull(asyncCreator);

            var entry = await source.FindEntryOrCreateValueAsync(
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
        public TVal FindValue<TVal>(string key, TVal missing = default)
                => source.FindValueAsync(key, missing).ExecuteSynchronously();
        public async Task<TVal> FindValueAsync<TVal>(string key, TVal missing = default)
        {
            var entry = await source.FindEntryAsync(key);
            return entry == null ? missing : entry.GetValue<TVal>();
        }
        public Task<ICacheEntry> FindEntryAsync(string key)
                => source.FindEntryOrCreateValueAsync(key);
        public TVal FindOrCreateValue<TVal>(string key, Func<TVal> creator, TimeSpan? cacheEntryTimeout = null, bool forceCreate = false)
                => source.FindOrCreateValueAsync<TVal>(key, () => Task.FromResult(creator()), cacheEntryTimeout, forceCreate).ExecuteSynchronously();
        public TVal FindOrPrimeValues<TVal>(string key, Func<IEnumerable<Tuple<string, TVal>>> primer, TimeSpan? cacheEntryTimeout = null)
        {
            var entry = source.FindEntryAsync(key).ExecuteSynchronously();
            if (entry != null && !entry.IsExpired) return entry.GetValue<TVal>();
            TVal ret = default;
            foreach (var t in primer())
            {
                source.FindOrCreateValue<TVal>(t.Item1, () => t.Item2, cacheEntryTimeout, true);
                if (t.Item1 == key)
                {
                    ret = t.Item2;
                }
            }
            return ret;
        }
    }














}
