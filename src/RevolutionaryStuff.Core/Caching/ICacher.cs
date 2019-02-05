using System;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    public interface ICacher
    {
        Task<ICacheEntry> FindOrCreateEntryAsync(string key, Func<string, Task<ICacheEntry>> asyncCreator = null, IFindOrCreateEntrySettings settings = null);

        Task RemoveAsync(string key);
    }
}
