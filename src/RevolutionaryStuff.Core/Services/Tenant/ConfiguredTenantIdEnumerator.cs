using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Core.Services.Tenant;

internal class ConfiguredTenantIdEnumerator(IServiceProvider serviceProvider, IOptions<ConfiguredTenantIdEnumerator.Config> configOptions, ILogger<ConfiguredTenantIdEnumerator> logger)
    : ITenantIdEnumerator, IConfiguredTenantIdEnumerator
{
    private readonly IServiceProvider ServiceProvider = serviceProvider;
    private readonly IOptions<Config> ConfigOptions = configOptions;
    private readonly ILogger Logger = logger;

    public class Config
    {
        public const string ConfigSectionName = "ConfiguredTenantEnumerator";

        public IList<string> TenantIds { get; set; }
    }

    public Task<IList<string>> GetTenantIdsAsync()
        => Task.FromResult(ConfigOptions.Value.TenantIds ?? []);

    Task<IList<string>> ITenantIdEnumerator.GetTenantIdsAsync()
        => Task.FromResult(ConfigOptions.Value.TenantIds ?? []);

    async Task ITenantIdEnumerator.ForEachScopedTenantAsync(Func<ITenantIdEnumerator.ExecuteArgs, Task> actAsync)
    {
        ArgumentNullException.ThrowIfNull(actAsync);
        var tenantIds = await GetTenantIdsAsync();
        foreach (var tenantId in tenantIds)
        {
            using var loggerScope = Logger.BeginScope("TenantId: {TenantId}", tenantId);
            using var scope = ServiceProvider.CreateScope();
            var scopedServiceProvider = scope.ServiceProvider;
            var tenantFinder = scopedServiceProvider.GetRequiredService<ITenantIdProvider>();
            if (tenantFinder is ISoftTenantIdProvider softTenantIdProvider)
            {
                softTenantIdProvider.TenantId = tenantId;
            }
            else
            {
                throw new InvalidOperationException("TenantFinder must implement ISoftTenantIdProvider");
            }
            await actAsync(new(scopedServiceProvider, tenantId));
        }
    }
}
