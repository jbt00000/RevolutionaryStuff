namespace RevolutionaryStuff.Core.Caching;

internal class ScopedCacher : ICacher
{
    private readonly ICacher Inner;

    private readonly string ScopeKey;

    public ScopedCacher(ICacher inner, params object[] keyParts)
    {
        Requires.NonNull(inner, nameof(inner));
        Inner = inner;
        ScopeKey = Cache.CreateKey(keyParts);
    }

    private string CreateScopedKey(string key)
        => Cache.CreateKey(key, ScopeKey);

    public Task<ICacheEntry> FindEntryOrCreateValueAsync(string key, Func<string, Task<CacheCreationResult>> asyncCreator = null, IFindOrCreateEntrySettings findOrCreateSettings = null)
        => Inner.FindEntryOrCreateValueAsync(CreateScopedKey(key), asyncCreator, findOrCreateSettings);

    public Task RemoveAsync(string key)
        => Inner.RemoveAsync(CreateScopedKey(key));
}
