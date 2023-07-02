using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;
internal class CosmosJsonEntityServer : BaseLoggingDisposable, IJsonEntityServer
{
    IJsonEntityContainer IJsonEntityServer.GetContainer(string containerId)
    {
        Requires.Text(containerId);
        return OnGetContainer(containerId);
    }

    public class Config
    {
        public const string ConfigSectionName = "CosmosJsonEntityServerConfig";
    }

    public sealed class CosmosJsonEntityServerConstructorArgs
    {
        internal readonly IOptions<Config> ConfigOptions;

        public CosmosJsonEntityServerConstructorArgs(IOptions<Config> configOptions)
        {
            ArgumentNullException.ThrowIfNull(configOptions);
            ConfigOptions = configOptions;
        }
    }

    protected readonly IOptions<Config> ConfigOptions;

    public CosmosJsonEntityServer(CosmosJsonEntityServerConstructorArgs constructorArgs, ILogger<CosmosJsonEntityServer> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(constructorArgs);

        ConfigOptions = constructorArgs.ConfigOptions;
    }

    protected virtual IJsonEntityContainer OnGetContainer(string containerId)
    {
        throw new NotImplementedException();
    }

    private static readonly IDictionary<string, CosmosClient> CosmosClientByConnectionString = new ConcurrentDictionary<string, CosmosClient>();
}
