using RevolutionaryStuff.Core.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    public abstract class BaseCacher : ICacher
    {
        protected readonly ReaderWriterLock RWL = new ReaderWriterLock();

        protected BaseCacher()
        { }

        protected virtual ICacheEntryRetentionPolicy DefaultCacheEntryRetentionPolicy
            => CacheEntryRetentionPolicy.Default;

        protected virtual ICacheEntry CreateEntry(CacheCreationResult res)
            => new CacheEntry(res.Val, res.RetentionPolicy);

        protected Task<ICacheEntry> FindEntryAsync(string key)
        {
            Requires.NonNull(key, nameof(key));
            using (RWL.UseRead())
            {
                return OnFindEntryAsync(key);
            }
        }

        protected abstract Task<ICacheEntry> OnFindEntryAsync(string key);

        protected Task WriteEntryAsync(string key, ICacheEntry entry)
        {
            Requires.NonNull(key, nameof(key));
            using (RWL.UseWrite())
            {
                return OnWriteEntryAsync(key, entry);
            }
        }

        protected abstract Task OnWriteEntryAsync(string key, ICacheEntry entry);

        Task<ICacheEntry> ICacher.FindEntryOrCreateValueAsync(string key, Func<string, Task<CacheCreationResult>> asyncCreator, IFindOrCreateEntrySettings findOrCreateSettings)
        {
            Requires.NonNull(key, nameof(key));

            return OnFindEntryOrCreateValueAsync(key, asyncCreator, findOrCreateSettings ?? FindOrCreateEntrySettings.Default);
        }

        protected virtual async Task<ICacheEntry> OnFindEntryOrCreateValueAsync(string key, Func<string, Task<CacheCreationResult>> asyncCreator, IFindOrCreateEntrySettings findOrCreateSettings)
        {
            using (var l = RWL.UseRead())
            {
                ICacheEntry entry = null;
                if (!findOrCreateSettings.ForceCreate)
                {
                    entry = await FindEntryAsync(key);
                }
                if (entry != null && !entry.IsExpired) return entry;
                if (asyncCreator == null) return null;
                using (l.UseWrite())
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
        }

        Task ICacher.RemoveAsync(string key)
        {
            Requires.NonNull(key, nameof(key));
            using (RWL.UseWrite())
            {
                return OnRemoveAsync(key);
            }
        }

        protected abstract Task OnRemoveAsync(string key);
    }
}
