using System;

namespace RevolutionaryStuff.Core.Caching
{
    public class CacheEntry<TVal> : ICacheEntry
    {
        public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;

        public DateTime ExpiresAtUtc { get; private set; }

        public string CreatedOn { get; } = Environment.MachineName;

        public TVal Value { get; private set; }

        object ICacheEntry.Value => Value;

        bool ICacheEntry.IsExpired => ExpiresAtUtc < DateTime.UtcNow;

        public CacheEntry(TVal val, TimeSpan? expiresIn=null)
        {
            Value = val;
            ExpiresAtUtc = expiresIn.HasValue ? CreatedAtUtc.Add(expiresIn.Value) : DateTime.MaxValue;
        }
    }
}
