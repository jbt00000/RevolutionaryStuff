using RevolutionaryStuff.Core.ApplicationParts;

namespace Traffk.StorageProviders.Providers.AzureBlobStorageProvider;

public partial class AzureBlobStorageProvider
{
    public class Config : IValidate
    {
        public const string ConfigSectionName = "AzureBlobStorageProviderStorageProviderConfig";
        public string ConnectionStringName { get; set; }
        public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;
        public string ContainerName { get; set; }

        public bool CaseSensitive { get; set; }
        public bool IsHierarchical { get; set; }

        void IValidate.Validate()
        {
            ArgumentNullException.ThrowIfNull(ConnectionStringName);
            ArgumentNullException.ThrowIfNull(ContainerName);
        }
    }
}
