using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Core.Services.Tenant;

internal class ConfiguredTenantIdProvider(IOptions<ConfiguredTenantIdProvider.Config> ConfigOptions) : ITenantIdProvider, IConfiguredTenantIdProvider
{
    public class Config
    {
        public const string ConfigSectionName = "ConfiguredTenantIdProvider";
        public string TenantId { get; set; }
    }

    Task<string> ITenantIdProvider.GetTenantIdAsync()
        => Task.FromResult(ConfigOptions.Value.TenantId);
}


