using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public class DefaultCosmosJsonEntityServerConfig
{
    public const string ConfigSectionName = "defaultCosmosJsonEntityServerConfig";

    public string ConnectionStringName { get; set; }

    public string DatabaseId { get; set; }
}

public abstract class DefaultCosmosJsonEntityServer<TTenantFinder> : CosmosJsonEntityServer<TTenantFinder>
    where TTenantFinder : ITenantFinder<string>
{
    private readonly IConnectionStringProvider ConnectionStringProvider;
    private readonly IOptions<DefaultCosmosJsonEntityServerConfig> MyConfigOptions;

    protected DefaultCosmosJsonEntityServer(IConnectionStringProvider connectionStringProvider, IOptions<DefaultCosmosJsonEntityServerConfig> myConfigOptions, TTenantFinder tenantFinder, CosmosJsonEntityServerConstructorArgs constructorArgs, ILogger logger)
        : base(tenantFinder, constructorArgs, logger)
    {
        ArgumentNullException.ThrowIfNull(connectionStringProvider);
        ArgumentNullException.ThrowIfNull(myConfigOptions);

        ConnectionStringProvider = connectionStringProvider;
        MyConfigOptions = myConfigOptions;
    }

    protected override string GetConnectionString()
        => ConnectionStringProvider.GetConnectionString(MyConfigOptions.Value.ConnectionStringName);

    protected override void ConfigureCosmosClientOptions(CosmosClientOptions clientOptions)
    {
        base.ConfigureCosmosClientOptions(clientOptions);
        clientOptions.MaxRetryAttemptsOnRateLimitedRequests = 10;
        clientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30);
    }

    protected override string GetDatabaseId(string containerId)
        => MyConfigOptions.Value.DatabaseId;
}
