using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public sealed class CosmosJsonEntityServerConstructorArgs
{
    internal readonly IServiceProvider ServiceProvider;
    internal readonly IOptions<CosmosJsonEntityServerConfig> ConfigOptions;

    public CosmosJsonEntityServerConstructorArgs(IServiceProvider serviceProvider, IOptions<CosmosJsonEntityServerConfig> configOptions)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(configOptions);

        ServiceProvider = serviceProvider;
        ConfigOptions = configOptions;
    }
}
