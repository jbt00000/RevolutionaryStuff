using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public abstract class DefaultCosmosJsonEntityServer<TTenantFinder> : CosmosJsonEntityServer
{
    private readonly IConnectionStringProvider ConnectionStringProvider;

    protected DefaultCosmosJsonEntityServer(IConnectionStringProvider connectionStringProvider, TTenantFinder tenantFinder, CosmosJsonEntityServerConstructorArgs constructorArgs, ILogger logger)
        : base(constructorArgs, logger)
    {
        ArgumentNullException.ThrowIfNull(connectionStringProvider);

        ConnectionStringProvider = connectionStringProvider;
    }

    protected override string GetConnectionString(string connectionStringName)
        => ConnectionStringProvider.GetConnectionString(connectionStringName);

    protected override void ConfigureCosmosClientOptions(CosmosClientOptions clientOptions)
    {
        base.ConfigureCosmosClientOptions(clientOptions);
        clientOptions.MaxRetryAttemptsOnRateLimitedRequests = 10;
        clientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30);
    }

    protected override IJsonEntityContainer CreateJsonEntityContainer(Container container)
        => new CosmosJsonEntityContainer(container, ConfigOptions, ServiceProvider.GetRequiredService<ILogger<CosmosJsonEntityContainer>>());
}
