using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    public abstract class BaseCacher : BaseCacher<CacheEntry>
    {
        protected virtual ICacheEntryRetentionPolicy DefaultCacheEntryRetentionPolicy
            => CacheEntryRetentionPolicy.Default;

        protected override CacheEntry CreateEntry(CacheCreationResult res)
            => new CacheEntry(res.Val, res.RetentionPolicy ?? DefaultCacheEntryRetentionPolicy);
    }

    public abstract class BaseCacher<T_CACHE_ENTRY> : ICacher where T_CACHE_ENTRY : ICacheEntry
    {
        //https://stackoverflow.com/questions/7612602/why-cant-i-use-the-await-operator-within-the-body-of-a-lock-statement
        private readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        protected BaseCacher()
        { }

        protected abstract T_CACHE_ENTRY CreateEntry(CacheCreationResult res);

        private long CacheHits_p;
        private long CacheMisses_p;
        private long CacheEntryGenerationMilliseconds_p;

        public long CacheHits
            => CacheHits_p;

        public long CacheMisses
            => CacheMisses_p;

        public TimeSpan CacheEntryGenerationTime
            => TimeSpan.FromMilliseconds(CacheEntryGenerationMilliseconds_p);

        public TimeSpan CacheEntryGenerationHitSavings
            => CacheMisses_p == 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(CacheEntryGenerationMilliseconds_p / CacheMisses_p * CacheHits_p);

        protected async Task<ICacheEntry> FindEntryAsync(string key)
        {
            Requires.NonNull(key, nameof(key));
            T_CACHE_ENTRY e;
            await Lock.WaitAsync();
            try
            {
                e = await OnFindEntryAsync(key);
            }
            finally
            {
                Lock.Release();
            }
            if (e == null)
            {
                Interlocked.Increment(ref CacheMisses_p);
            }
            else
            {
                Interlocked.Increment(ref CacheHits_p);
            }
            return e;
        }

        protected abstract Task<T_CACHE_ENTRY> OnFindEntryAsync(string key);

        protected async Task WriteEntryAsync(string key, T_CACHE_ENTRY entry)
        {
            Requires.NonNull(key, nameof(key));
            await Lock.WaitAsync();
            try
            {
                await OnWriteEntryAsync(key, entry);
            }
            finally
            {
                Lock.Release();
            }
        }

        protected abstract Task OnWriteEntryAsync(string key, T_CACHE_ENTRY entry);

        Task<ICacheEntry> ICacher.FindEntryOrCreateValueAsync(string key, Func<string, Task<CacheCreationResult>> asyncCreator, IFindOrCreateEntrySettings findOrCreateSettings)
        {
            Requires.NonNull(key, nameof(key));
            return OnFindEntryOrCreateValueAsync(key, asyncCreator, findOrCreateSettings ?? FindOrCreateEntrySettings.Default);
        }

        protected virtual async Task<ICacheEntry> OnFindEntryOrCreateValueAsync(string key, Func<string, Task<CacheCreationResult>> asyncCreator, IFindOrCreateEntrySettings findOrCreateSettings)
        {
            T_CACHE_ENTRY entry = default(T_CACHE_ENTRY);
            if (!findOrCreateSettings.ForceCreate)
            {
                entry = (T_CACHE_ENTRY)await FindEntryAsync(key);
            }
            if (entry != null && !entry.IsExpired) return entry;
            if (asyncCreator == null) return null;

            if (entry != null)
            {
                await RemoveAsync(key);
            }
            var sw = new Stopwatch();
            sw.Start();
            var creationResult = await asyncCreator(key);
            sw.Stop();
            Interlocked.Add(ref CacheEntryGenerationMilliseconds_p, sw.ElapsedMilliseconds);
            entry = CreateEntry(creationResult);
            await WriteEntryAsync(key, entry);
            return entry;
        }

        public async Task RemoveAsync(string key)
        {
            Requires.NonNull(key, nameof(key));
            await Lock.WaitAsync();
            try
            {
                await OnRemoveAsync(key);
            }
            finally
            {
                Lock.Release();
            }
        }

        protected abstract Task OnRemoveAsync(string key);
    }
}
