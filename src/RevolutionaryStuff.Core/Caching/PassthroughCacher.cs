using System;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    internal class PassthroughCacher : ICacher
    {
        async Task<ICacheEntry> ICacher.FindOrCreateEntryAsync(string key, Func<string, Task<ICacheEntry>> asyncCreator, IFindOrCreateEntrySettings settings)
            => await asyncCreator(key);

        Task ICacher.RemoveAsync(string key)
            => Task.CompletedTask;
    }
}
