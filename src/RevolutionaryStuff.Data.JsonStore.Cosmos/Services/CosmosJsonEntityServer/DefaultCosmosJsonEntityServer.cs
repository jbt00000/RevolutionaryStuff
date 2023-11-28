using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public abstract class DefaultCosmosJsonEntityServer<TTenantFinder> : CosmosJsonEntityServer<TTenantFinder>
    where TTenantFinder : ITenantFinder<string>
{
    private readonly IConnectionStringProvider ConnectionStringProvider;

    protected DefaultCosmosJsonEntityServer(IConnectionStringProvider connectionStringProvider, TTenantFinder tenantFinder, CosmosJsonEntityServerConstructorArgs constructorArgs, ILogger logger)
        : base(tenantFinder, constructorArgs, logger)
    {
        ArgumentNullException.ThrowIfNull(connectionStringProvider);

        ConnectionStringProvider = connectionStringProvider;
    }

    protected override string GetConnectionString(string conectionStringName)
        => ConnectionStringProvider.GetConnectionString(conectionStringName);

    protected override void ConfigureCosmosClientOptions(CosmosClientOptions clientOptions)
    {
        base.ConfigureCosmosClientOptions(clientOptions);
        clientOptions.MaxRetryAttemptsOnRateLimitedRequests = 10;
        clientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30);
    }
}
