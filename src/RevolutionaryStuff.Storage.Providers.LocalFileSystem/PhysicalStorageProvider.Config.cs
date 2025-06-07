namespace Traffk.StorageProviders.Providers.PhysicalStorage;

public partial class PhysicalStorageProvider
{
    public class Config
    {
        public const string ConfigSectionName = "PhysicalStorageProviderConfig";

        public bool? CaseSensitive { get; set; }

        public string FolderSeparator { get; set; }

        public string RootFolder { get; set; }
    }
}
