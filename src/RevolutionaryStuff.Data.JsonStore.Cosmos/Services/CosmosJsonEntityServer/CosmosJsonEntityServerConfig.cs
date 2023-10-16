using Microsoft.Azure.Cosmos;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public class CosmosJsonEntityServerConfig
{
    public const string ConfigSectionName = "CosmosJsonEntityServerConfig";

    public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;

    public ConnectionMode ConnectionMode { get; set; } = ConnectionMode.Direct;

    public string ApplicationName { get; set; }
}
