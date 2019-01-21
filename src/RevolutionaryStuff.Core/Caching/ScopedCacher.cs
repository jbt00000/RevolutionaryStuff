using System;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Caching
{
    internal class ScopedCacher : ICacher
    {
        private readonly ICacher Inner;
        private readonly string ScopeKey;

        public ScopedCacher(ICacher inner, params object[] keyParts)
        {
            Requires.NonNull(inner, nameof(inner));
            Inner = inner;
            ScopeKey = Cache.CreateKey(keyParts);
        }

        private string CreateScopedKey(string key)
            => Cache.CreateKey(key, ScopeKey);

        Task<ICacheEntry> ICacher.FindOrCreateEntryAsync(string key, Func<string, Task<ICacheEntry>> asyncCreator, bool forceCreate)
            => Inner.FindOrCreateEntryAsync(CreateScopedKey(key), asyncCreator, forceCreate);

        Task ICacher.RemoveAsync(string key)
            => Inner.RemoveAsync(CreateScopedKey(key));
    }
}
