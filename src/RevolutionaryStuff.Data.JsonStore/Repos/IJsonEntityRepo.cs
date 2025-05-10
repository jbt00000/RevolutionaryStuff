using System.Linq.Expressions;
using RevolutionaryStuff.Data.JsonStore.Entities;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Repos;

public interface IJsonEntityRepo<TBaseEntity>
    where TBaseEntity : JsonEntity
{
    IJsonEntityContainer GetContainer<TItem>()
        where TItem : TBaseEntity;

    IQueryable<TItem> GetQueryable<TItem>(QueryOptions? requestOptions = null)
        where TItem : TBaseEntity;

    IQueryable<TItem> GetQueryable<TItem>(Expression<Func<TItem, bool>> predicate, QueryOptions? requestOptions = null)
        where TItem : TBaseEntity
        => GetQueryable<TItem>(requestOptions).Where(predicate);

    Task CreateItemAsync<TItem>(TItem entity)
        where TItem : TBaseEntity;

    Task CreateItemsAsync<TItem>(IList<TItem> entities)
        where TItem : TBaseEntity;

    Task<TItem> UpdateItemAsync<TItem>(TItem entity, Func<TItem, Task> amendAsync)
        where TItem : TBaseEntity;

    Task<TItem> UpdateItemAsync<TItem>(TItem entity, Action<TItem> amend)
        where TItem : TBaseEntity;

    Task<TItem> ReplaceItemAsync<TItem>(TItem item, Func<TItem, Task<TItem>> amendAsync)
        where TItem : TBaseEntity, new();

    Task<bool> PatchItemAsync<TItem>(string id, string partitionKey, Expression<Func<TItem, object>> property, object updatedValue, PatchOperationTypeEnum op = PatchOperationTypeEnum.Add, string eTag = null)
        where TItem : TBaseEntity;

    Task PatchItemAsync<TItem>(TItem entity, Expression<Func<TItem, object>> property, object updatedValue, PatchOperationTypeEnum op = PatchOperationTypeEnum.Add)
        where TItem : TBaseEntity
        => GetContainer<TItem>().PatchItemAsync(entity, property, updatedValue, op);

    Task TouchItemAsync<TItem>(string id, string partitionKey, string? propertyName = null)
        where TItem : TBaseEntity;

    Task TouchItemAsync<TItem>(TItem item)
        where TItem : TBaseEntity
        => TouchItemAsync<TItem>(item.Id, item.PartitionKey);

}
