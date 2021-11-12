namespace RevolutionaryStuff.Core.Caching;

public interface ICacheEntryRetentionPolicy
{
    TimeSpan? CacheEntryTimeout { get; }
}
