using RevolutionaryStuff.Data.JsonStore.Entities;
using RevolutionaryStuff.Data.JsonStore.Repos;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Repos;
public interface ICosmosJsonEntityRepo<TBaseEntity> : IJsonEntityRepo<TBaseEntity>
    where TBaseEntity : JsonEntity
{
    Task<bool> CreateItemIfNotExistsAsync<TItem>(TItem entity)
        where TItem : TBaseEntity;
}
