using System;

namespace RevolutionaryStuff.Core.Caching
{
    public interface IFindOrCreateEntrySettings
    {
        bool ForceCreate { get; }

        TimeSpan? CacheEntryTimeout { get; }
    }
}
