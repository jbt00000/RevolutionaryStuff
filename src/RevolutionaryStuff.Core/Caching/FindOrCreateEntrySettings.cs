using System;

namespace RevolutionaryStuff.Core.Caching
{
    public class FindOrCreateEntrySettings : IFindOrCreateEntrySettings
    {
        public static readonly IFindOrCreateEntrySettings Default = new FindOrCreateEntrySettings();

        public bool ForceCreate { get; set; } = false;

        public TimeSpan? CacheEntryTimeout { get; set; } = null;

        public FindOrCreateEntrySettings()
        { }

        public FindOrCreateEntrySettings(bool forceCreate, TimeSpan? cacheEntryTimeout = null)
        {
            ForceCreate = forceCreate;
            CacheEntryTimeout = cacheEntryTimeout;
        }
    }
}
