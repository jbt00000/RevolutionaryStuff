using Nito.AsyncEx;
using System;
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
        protected readonly AsyncReaderWriterLock RWL = new AsyncReaderWriterLock();

        protected BaseCacher()
        { }

        protected abstract T_CACHE_ENTRY CreateEntry(CacheCreationResult res);

        protected async Task<ICacheEntry> FindEntryAsync(string key)
        {
            Requires.NonNull(key, nameof(key));
            using (await RWL.ReaderLockAsync())
            {
                return await OnFindEntryAsync(key);
            }
        }

        protected abstract Task<T_CACHE_ENTRY> OnFindEntryAsync(string key);

        protected async Task WriteEntryAsync(string key, T_CACHE_ENTRY entry)
        {
            Requires.NonNull(key, nameof(key));
            using (await RWL.WriterLockAsync())
            {
                await OnWriteEntryAsync(key, entry);
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
            using (var l = await RWL.ReaderLockAsync())
            {
                if (!findOrCreateSettings.ForceCreate)
                {
                    entry = (T_CACHE_ENTRY)await FindEntryAsync(key);
                }
                if (entry != null && !entry.IsExpired) return entry;
                if (asyncCreator == null) return null;
            }
            using (await RWL.WriterLockAsync())
            {
                if (entry != null)
                {
                    await OnRemoveAsync(key);
                }
                var creationResult = await asyncCreator(key);
                entry = CreateEntry(creationResult);
                await OnWriteEntryAsync(key, entry);
                return entry;
            }
        }

        async Task ICacher.RemoveAsync(string key)
        {
            Requires.NonNull(key, nameof(key));
            using (await RWL.WriterLockAsync())
            {
                await OnRemoveAsync(key);
            }
        }

        protected abstract Task OnRemoveAsync(string key);
    }
}
