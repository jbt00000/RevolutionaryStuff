using System.Threading;
using RevolutionaryStuff.Data.Cosmos;
using RevolutionaryStuff.Data.JsonStore.Entities;
using RevolutionaryStuff.Data.JsonStore.Repos;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Repos;

public abstract class CosmosJsonEntityRepo<TBaseEntity> : JsonEntityRepo<TBaseEntity>, ICosmosJsonEntityRepo<TBaseEntity>
    where TBaseEntity : JsonEntity
{

    protected CosmosJsonEntityRepo(IList<string> containerIds, CosmosRepoConstructorArgs constructorArgs)
    : base(containerIds, constructorArgs.BaseRepoConstructorArgs)
    { }

    protected override Task<IReadOnlyList<T>> GetAllItemsAsync<T>(IQueryable<T> q, CancellationToken cancellationToken)
        => CosmosHelpers.GetAllItemsAsync(q, cancellationToken);
}
