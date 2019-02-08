using System;

namespace RevolutionaryStuff.Core.Caching
{
    public class CacheEntry : ICacheEntry
    {
        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

        public DateTimeOffset ExpiresAt { get; private set; }

        public string CreatedOn { get; } = Environment.MachineName;

        public object Value { get; private set; }

        object ICacheEntry.Value => Value;

        bool ICacheEntry.IsExpired => ExpiresAt < DateTimeOffset.Now;

        public CacheEntry(object val, TimeSpan? expiresIn=null)
        {
            Value = val;
            ExpiresAt = expiresIn.HasValue ? CreatedAt.Add(expiresIn.Value) : DateTimeOffset.MaxValue;
        }

        public CacheEntry(object val, ICacheEntryRetentionPolicy settings)
            : this(val, settings?.CacheEntryTimeout)
        { }

        public static ICacheEntry Create(object val, TimeSpan? expiresIn = null)
            => new CacheEntry(val, expiresIn);
    }
}
