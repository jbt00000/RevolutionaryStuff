using System;

namespace RevolutionaryStuff.Core.Caching
{
    public class CacheEntry<TVal> : ICacheEntry
    {
        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

        public DateTimeOffset ExpiresAt { get; private set; }

        public string CreatedOn { get; } = Environment.MachineName;

        public TVal Value { get; private set; }

        object ICacheEntry.Value => Value;

        bool ICacheEntry.IsExpired => ExpiresAt < DateTimeOffset.Now;

        public CacheEntry(TVal val, TimeSpan? expiresIn=null)
        {
            Value = val;
            ExpiresAt = expiresIn.HasValue ? CreatedAt.Add(expiresIn.Value) : DateTimeOffset.MaxValue;
        }

        public static ICacheEntry Create(TVal val, TimeSpan? expiresIn = null)
            => new CacheEntry<TVal>(val, expiresIn);
    }
}
