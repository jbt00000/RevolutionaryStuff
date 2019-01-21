using System;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    public interface ICacher
    {
        Task<ICacheEntry> FindOrCreateEntryAsync(string key, Func<string, Task<ICacheEntry>> asyncCreator = null, bool forceCreate = false);

        Task RemoveAsync(string key);
    }
}
