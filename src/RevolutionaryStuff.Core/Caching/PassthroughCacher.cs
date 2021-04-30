using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    internal class PassthroughCacher : BaseCacher, ILocalCacher
    {
        protected override Task OnWriteEntryAsync(string key, CacheEntry entry)
            => Task.CompletedTask;

        protected override Task<CacheEntry> OnFindEntryAsync(string key)
            => Task.FromResult<CacheEntry>(null);

        protected override Task OnRemoveAsync(string key)
            => Task.CompletedTask;
    }
}
