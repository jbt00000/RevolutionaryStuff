using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Azure;

public static class HierarchicalKeyVaultSecretManagerHelpers
{
    public static void SetupProgramWithHierarchicalKeyVaultSecretManager(this IHostApplicationBuilder builder, string vaultConfigName = null)
    {
        VaultConfig config = new();
        builder.Configuration.Bind(vaultConfigName ?? VaultConfig.ConfigSectionName, config);
        Requires.Valid(config);
        builder.Configuration.AddAzureKeyVault(
            new Uri(config.Url),
            new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeManagedIdentityCredential = builder.Environment.IsDevelopment()
            }),
            new global::Azure.Extensions.AspNetCore.Configuration.Secrets.AzureKeyVaultConfigurationOptions()
            {
                ReloadInterval = config.ReloadInterval,
                Manager = new HierarchicalKeyVaultSecretManager()
            });
    }

    internal class VaultConfig : IValidate
    {
        public const string ConfigSectionName = "vault";
        public string Url { get; set; }
        public TimeSpan ReloadInterval { get; set; } = TimeSpan.FromMinutes(5);
        public List<string> SecretPrefixes { get; set; }

        public bool HasValidUrl
            => Uri.TryCreate(Url, UriKind.Absolute, out var _);

        public void Validate()
        => new Uri(Url, UriKind.Absolute);
    }
}
