using Microsoft.Extensions.Options;
using RevolutionaryStuff.Azure.Services.Authentication;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public sealed record CosmosJsonEntityServerConstructorArgs(
    IAzureTokenCredentialProvider AzureTokenCredentialProvider,
    IServiceProvider ServiceProvider,
    IOptions<CosmosJsonEntityServerConfig> ConfigOptions
);
