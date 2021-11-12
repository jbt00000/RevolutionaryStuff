namespace RevolutionaryStuff.Core.Caching;

/// <summary>
/// Simple interface to store and retrieve cached items
/// </summary>
public interface ICacher
{
    /// <summary>
    /// Returns a cache entry if one already existed, if not, create item, cache it, then return
    /// </summary>
    /// <param name="key">The cache key.  Must not be null</param>
    /// <param name="asyncCreator">Function called to creaate a new value.  When null and if the item is not cached, a null ICacheEntry will be returned</param>
    /// <param name="findOrCreateSettings">Optional settings that can get passed into the cache</param>
    /// <returns>The found cache entry</returns>
    Task<ICacheEntry> FindEntryOrCreateValueAsync(
        string key,
        Func<string, Task<CacheCreationResult>> asyncCreator = null,
        IFindOrCreateEntrySettings findOrCreateSettings = null);

    /// <summary>
    /// Remvoe an item from the cache
    /// </summary>
    /// <param name="key">The cache key.  Must not be null</param>
    /// <returns>Completion</returns>
    Task RemoveAsync(string key);
}

public interface ILocalCacher : ICacher { }
public interface IRemoteCacher : ICacher { }
