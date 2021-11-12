namespace RevolutionaryStuff.Core.Caching;

public class CacheEntry : ICacheEntry
{
    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset ExpiresAt { get; }

    public string CreatedOn { get; }

    public object Value { get; }

    public virtual bool IsExpired
        => ExpiresAt < DateTimeOffset.Now;

    public CacheEntry(object val, TimeSpan? expiresIn = null, DateTimeOffset? createdAt = null, string createdOn = null)
    {
        Value = val;
        CreatedAt = createdAt.HasValue ? createdAt.Value : DateTimeOffset.UtcNow;
        ExpiresAt = expiresIn.HasValue ? CreatedAt.Add(expiresIn.Value) : DateTimeOffset.MaxValue;
        CreatedOn = createdOn ?? Environment.MachineName;
    }

    public CacheEntry(object val, ICacheEntryRetentionPolicy settings)
        : this(val, settings?.CacheEntryTimeout)
    { }
}
