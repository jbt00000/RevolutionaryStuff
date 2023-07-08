using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public abstract class CosmosJsonEntityServer<TTenantFinder> : BaseLoggingDisposable, IJsonEntityServer
    where TTenantFinder : ITenantFinder<string>
{
    private static readonly IDictionary<string, CosmosClient> CosmosClientByTenantId = new ConcurrentDictionary<string, CosmosClient>();
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
        ServiceProvider = constructorArgs.ServiceProvider;
        ConfigOptions = constructorArgs.ConfigOptions;
    }

    protected string TenantId
        => TenantIdField ??= TenantFinder.GetTenantIdAsync().ExecuteSynchronously();

    /// <summary>
    /// This should take into account the current TenantId
    /// </summary>
    /// <returns>The connection string for the underlying CosmosClient</returns>
    protected abstract string GetConnectionString();

    /// <summary>
    /// This should take into account the current TenantId
    /// </summary>
    /// <returns>CosmosClientOptions associated with the to be created CosmosClient</returns>
    protected abstract CosmosClientOptions CreateCosmosClientOptions();

    /// <summary>
    /// This should take into account the current TenantId
    /// </summary>
    /// <param name="containerId">The inbound containerId</param>
    /// <returns>The associated databaseId</returns>
    protected abstract string GetDatabaseId(string containerId);

    protected virtual IJsonEntityContainer CreateJsonEntityContainer(Container container)
        => new CosmosJsonEntityContainer(container, TenantId, ServiceProvider.GetRequiredService<IOptions<CosmosJsonEntityContainerConfig>>(), ServiceProvider.GetRequiredService<ILogger<CosmosJsonEntityContainer>>());

    private CosmosClient CosmosClient
    {
        get
        {
            if (CosmosClientField == null && !CosmosClientByTenantId.TryGetValue(TenantId, out CosmosClientField))
            {
                var connectionString = GetConnectionString();
                _ = TenantId; //store locally before the lock as this may execute synchronously
                lock (CosmosClientByTenantId)
                {
                    CosmosClientField = CosmosClientByTenantId.FindOrCreate(
                        TenantId,
                        () => new(connectionString, CreateCosmosClientOptions()) //no part of the CosmosClient can be scoped
                        );
                }
            }
            return CosmosClientField;
        }
    }

    IJsonEntityContainer IJsonEntityServer.GetContainer(string containerId)
    {
        Requires.Text(containerId);

        return JsonEntityContainerByRepositoryId.FindOrCreate(containerId, () => 
        {
            var databaseId = GetDatabaseId(containerId);
            Requires.Text(databaseId);

            var container = CosmosClient.GetContainer(databaseId, containerId);

            return CreateJsonEntityContainer(container);
        });
    }
}
