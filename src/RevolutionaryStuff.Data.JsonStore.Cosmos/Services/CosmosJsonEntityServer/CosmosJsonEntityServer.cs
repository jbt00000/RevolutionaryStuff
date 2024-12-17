using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Azure.Services.Authentication;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.Cosmos;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public abstract class CosmosJsonEntityServer<TTenantFinder> : BaseLoggingDisposable, IJsonEntityServer
    where TTenantFinder : ITenantFinder<string>
{
    private static readonly IDictionary<string, CosmosClient> CosmosClientByTenantId = new ConcurrentDictionary<string, CosmosClient>();
    private readonly IAzureTokenCredentialProvider AzureTokenCredentialProvider;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly TTenantFinder TenantFinder;
    protected readonly IOptions<CosmosJsonEntityServerConfig> ConfigOptions;
    private CosmosClient? CosmosClientField;
    private string? TenantIdField;
    private readonly IDictionary<string, IJsonEntityContainer> JsonEntityContainerByRepositoryId = new ConcurrentDictionary<string, IJsonEntityContainer>();

    protected CosmosJsonEntityServer(TTenantFinder tenantFinder, CosmosJsonEntityServerConstructorArgs constructorArgs, ILogger logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(tenantFinder);
        ArgumentNullException.ThrowIfNull(constructorArgs);

        TenantFinder = tenantFinder;
        AzureTokenCredentialProvider = constructorArgs.AzureTokenCredentialProvider;
        ServiceProvider = constructorArgs.ServiceProvider;
        ConfigOptions = constructorArgs.ConfigOptions;
    }

    protected string TenantId
        => TenantIdField ??= TenantFinder.GetTenantIdAsync().ExecuteSynchronously();

    /// <summary>
    /// This should take into account the current TenantId
    /// </summary>
    /// <returns>The connection string for the underlying CosmosClient</returns>
    protected abstract string GetConnectionString(string connectionStringName);

    /// <summary>
    /// This should take into account the current TenantId
    /// </summary>
    /// <returns>CosmosClientOptions associated with the to be created CosmosClient</returns>
    private CosmosClientOptions CreateCosmosClientOptions()
    {
        var cco = new CosmosClientOptions();
        ConfigureCosmosClientOptions(cco);
        return cco;
    }

    protected virtual void ConfigureCosmosClientOptions(CosmosClientOptions cosmosClientOptions)
    {
        var config = ConfigOptions.Value;
        cosmosClientOptions.Serializer = new DefaultCosmosEntitySerializer();
        cosmosClientOptions.ConnectionMode = config.ConnectionMode;
        cosmosClientOptions.ApplicationName = config.ApplicationName ?? RevolutionaryStuffCoreConfig.GetApplicationName(ServiceProvider.GetRequiredService<IConfiguration>());
    }

    protected virtual IJsonEntityContainer CreateJsonEntityContainer(Container container)
        => new CosmosJsonEntityContainer(container, TenantId, ConfigOptions, ServiceProvider.GetRequiredService<ILogger<CosmosJsonEntityContainer>>());

    private CosmosClient CosmosClient
    {
        get
        {
            if (CosmosClientField == null && !CosmosClientByTenantId.TryGetValue(TenantId, out CosmosClientField))
            {
                var config = ConfigOptions.Value;
                var connectionString = GetConnectionString(config.ConnectionStringName);
                _ = TenantId; //store locally before the lock as this may execute synchronously
                lock (CosmosClientByTenantId)
                {
                    CosmosClientField = CosmosClientByTenantId.FindOrCreate(
                        TenantId,
                        () => ConstructCosmosClient(connectionString, CreateCosmosClientOptions())
                        );
                }
            }
            return CosmosClientField;
        }
    }

    protected virtual CosmosClient ConstructCosmosClient(string connectionString, CosmosClientOptions cosmosClientOptions)
        => CosmosHelpers.ConstructCosmosClient(
            new CosmosHelpers.CosmosClientAuthenticationSettings(connectionString, AzureTokenCredentialProvider, ConfigOptions.Value.AuthenticateWithWithDefaultAzureCredentials, true),
            cosmosClientOptions);

    /// <summary>
    /// You may want to override this if you want to do things such as changing the database based on the Tenant
    /// </summary>
    /// <param name="containerKey">The single string key for a container which ultimately maps its config</param>
    /// <returns>The container config</returns>
    protected virtual CosmosJsonEntityServerConfig.ContainerConfig GetContainerConfig(string containerKey)
        => ConfigOptions.Value.ContainerConfigByContainerKey?.GetValue(containerKey);

    IJsonEntityContainer IJsonEntityServer.GetContainer(string containerId)
    {
        Requires.Text(containerId);

        return JsonEntityContainerByRepositoryId.FindOrCreate(containerId, () =>
        {
            var containerInfo = GetContainerConfig(containerId);
            ArgumentNullException.ThrowIfNull(containerInfo, $"Cannot find containerInfo for containerKey=[{containerId}]");
            var container = CosmosClient.GetContainer(containerInfo.DatabaseConfig.DatabaseId, containerInfo.ContainerId);
            return CreateJsonEntityContainer(container);
        });
    }
}
