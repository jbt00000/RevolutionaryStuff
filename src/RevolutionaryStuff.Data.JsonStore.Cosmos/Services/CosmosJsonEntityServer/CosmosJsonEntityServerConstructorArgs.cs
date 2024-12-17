using Microsoft.Extensions.Options;
using RevolutionaryStuff.Azure.Services.Authentication;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public sealed class CosmosJsonEntityServerConstructorArgs
{
    internal readonly IAzureTokenCredentialProvider AzureTokenCredentialProvider;
    internal readonly IServiceProvider ServiceProvider;
    internal readonly IOptions<CosmosJsonEntityServerConfig> ConfigOptions;

    public CosmosJsonEntityServerConstructorArgs(IAzureTokenCredentialProvider azureTokenCredentialProvider, IServiceProvider serviceProvider, IOptions<CosmosJsonEntityServerConfig> configOptions)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(configOptions);
        AzureTokenCredentialProvider = azureTokenCredentialProvider;
        ServiceProvider = serviceProvider;
        ConfigOptions = configOptions;
    }
}
