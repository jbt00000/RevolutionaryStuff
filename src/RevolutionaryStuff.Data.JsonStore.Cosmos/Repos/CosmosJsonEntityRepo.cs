using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Data.Cosmos;
using RevolutionaryStuff.Data.JsonStore.Entities;
using RevolutionaryStuff.Data.JsonStore.Repos;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Repos;

public abstract class CosmosJsonEntityRepo<TBaseEntity> : JsonEntityRepo<TBaseEntity>, ICosmosJsonEntityRepo<TBaseEntity>
    where TBaseEntity : JsonEntity
{

    protected CosmosJsonEntityRepo(IList<string> containerIds, CosmosRepoConstructorArgs constructorArgs, ILogger logger)
    : base(containerIds, constructorArgs.BaseRepoConstructorArgs, logger)
    { }

    protected override Task<IReadOnlyList<T>> GetAllItemsAsync<T>(IQueryable<T> q, CancellationToken cancellationToken)
        => CosmosHelpers.GetAllItemsAsync(q, cancellationToken);

    async Task<bool> ICosmosJsonEntityRepo<TBaseEntity>.CreateItemIfNotExistsAsync<TItem>(TItem entity)
    {
        try
        {
            await I.CreateItemAsync(entity);
            return true;
        }
        catch (CosmosException cex) when (cex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return false;
        }
    }
}
