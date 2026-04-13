using Microsoft.Azure.Cosmos;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public interface ICosmosJsonEntityServer : IJsonEntityServer
{
     CosmosClient CosmosClient { get; }
}
