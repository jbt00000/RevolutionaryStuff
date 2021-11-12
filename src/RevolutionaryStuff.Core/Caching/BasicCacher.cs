namespace RevolutionaryStuff.Core.Caching;

public class BasicCacher : BaseCacher, ILocalCacher
{
    //This is internal to support unit testing
    internal readonly IDictionary<string, CacheEntry> CacheEntryByCacheKey = new Dictionary<string, CacheEntry>();

    protected readonly int MaxEntries;

    public BasicCacher(int? maxEntries = null)
    {
        MaxEntries = maxEntries.GetValueOrDefault(int.MaxValue);
    }

    protected override Task<CacheEntry> OnFindEntryAsync(string key)
        => Task.FromResult(CacheEntryByCacheKey.FindOrDefault(key));

    protected override Task OnWriteEntryAsync(string key, CacheEntry entry)
    {
        if (CacheEntryByCacheKey.Count >= MaxEntries)
        {
            var keys = CacheEntryByCacheKey.Keys.ToList();
            keys.Shuffle();
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
