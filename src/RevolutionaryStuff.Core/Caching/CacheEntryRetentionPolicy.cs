namespace RevolutionaryStuff.Core.Caching;

public class CacheEntryRetentionPolicy : ICacheEntryRetentionPolicy
{
    public static readonly ICacheEntryRetentionPolicy Default = new CacheEntryRetentionPolicy { CacheEntryTimeout = null };

    public TimeSpan? CacheEntryTimeout { get; set; }

    public CacheEntryRetentionPolicy(TimeSpan? cacheEntryTimeout = null)
    {
        CacheEntryTimeout = cacheEntryTimeout;
    }
}
