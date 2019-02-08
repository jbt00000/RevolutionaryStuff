using System;

namespace RevolutionaryStuff.Core.Caching
{
    public class FindOrCreateEntrySettings : IFindOrCreateEntrySettings
    {
        public static readonly IFindOrCreateEntrySettings Default = new FindOrCreateEntrySettings();
        public static readonly IFindOrCreateEntrySettings ForceCreateTrue = new FindOrCreateEntrySettings(true);
        public static readonly IFindOrCreateEntrySettings ForceCreateFalse = new FindOrCreateEntrySettings(false);

        public bool ForceCreate { get; set; } = false;

        public FindOrCreateEntrySettings(bool forceCreate=false)
        {
            ForceCreate = forceCreate;
        }
    }
}
