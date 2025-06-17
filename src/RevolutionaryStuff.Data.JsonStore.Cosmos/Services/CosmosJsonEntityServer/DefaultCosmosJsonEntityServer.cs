using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.Tenant;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public abstract class DefaultCosmosJsonEntityServer : CosmosJsonEntityServer
{
    public record DefaultCosmosJsonEntityServerConstructorArgs(
        IConnectionStringProvider ConnectionStringProvider, ITenantIdProvider TenantIdProvider, CosmosJsonEntityServerConstructorArgs BaseConstructorArgs)
    { }

    private readonly IConnectionStringProvider ConnectionStringProvider;
    private readonly ITenantIdProvider TenantIdProvider;

    protected DefaultCosmosJsonEntityServer(DefaultCosmosJsonEntityServerConstructorArgs constructorArgs, ILogger logger)
        : base(constructorArgs.BaseConstructorArgs, logger)
    {
        ArgumentNullException.ThrowIfNull(constructorArgs.ConnectionStringProvider);

        ConnectionStringProvider = constructorArgs.ConnectionStringProvider;
        TenantIdProvider = constructorArgs.TenantIdProvider;
    }

    protected override string GetTenantId()
        => TenantIdProvider.GetTenantId();

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
