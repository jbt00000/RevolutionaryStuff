using System;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    internal class PassthroughCacher : BaseCacher
    {
        protected override Task OnWriteEntryAsync(string key, ICacheEntry entry)
            => Task.CompletedTask;

        protected override Task<ICacheEntry> OnFindEntryAsync(string key)
            => Task.FromResult<ICacheEntry>(null);

        protected override Task OnRemoveAsync(string key)
            => Task.CompletedTask;
    }
}
