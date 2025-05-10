using System.Linq.Expressions;
using System.Threading;
using RevolutionaryStuff.Data.JsonStore.Entities;

namespace RevolutionaryStuff.Data.JsonStore.Store;

public interface IJsonEntityContainer
{
    /// <summary>
    /// Return the ID or Name of this container
    /// </summary>
    string ContainerId { get; }

    /// <summary>
    /// Delete the specified item as an async operation
    /// 
    /// When the type is annotated with SoftDeleteViaDataTypeChangeAttribute, a SoftDelete is performed
    /// </summary>
    /// <typeparam name="TItem">The type of the entity that is being deleted</typeparam>
    /// <param name="id">The id of the entity</param>
    /// <param name="partitionKey">The partition key of the entity</param>
    /// <param name="options"> (Optional) The options for the delete operation.</param>
    /// <param name="cancellationToken">(Optional) System.Threading.CancellationToken representing request cancellation.</param>
    /// <returns>A waitable task</returns>
    Task DeleteItemAsync<TItem>(string id, string partitionKey, DeleteOptions? options = null, CancellationToken cancellationToken = default) where TItem : JsonEntity;

    Task CreateItemAsync<TItem>(TItem item, CancellationToken cancellationToken = default) where TItem : JsonEntity;

    Task<TItem> GetItemByIdAsync<TItem>(string id, string? partitionKey, CancellationToken cancellationToken = default) where TItem : JsonEntity;

    IQueryable<TItem> GetQueryable<TItem>(QueryOptions? requestOptions = null) where TItem : JsonEntity;

    Task<IReadOnlyList<TItem>> GetItemsAsync<TItem>(Expression<Func<TItem, bool>>? predicate = null, QueryOptions? options = null, CancellationToken cancellationToken = default) where TItem : JsonEntity;

    Task<bool> PatchItemAsync<TItem>(string id, string partitionKey, IList<PatchOperation> patches, string? eTag = null, CancellationToken cancellationToken = default) where TItem : JsonEntity;

    Task PatchItemAsync<TItem>(TItem item, Func<TItem, CancellationToken, Task<IList<PatchOperation>>> getPatchesAsync, CancellationToken cancellationToken = default) where TItem : JsonEntity;

    Task<TItem> UpdateItemAsync<TItem>(TItem item, Func<TItem, Task<bool>> amendAsync, CancellationToken cancellationToken = default) where TItem : JsonEntity;

    Task<TItem> ReplaceItemAsync<TItem>(TItem item, Func<TItem, Task<TItem>> amendAsync, CancellationToken cancellationToken = default) where TItem : JsonEntity;


    #region Default Implementations

    Task DeleteItemAsync<TItem>(TItem item, DeleteOptions? options = null, CancellationToken cancellationToken = default) where TItem : JsonEntity
        => DeleteItemAsync<TItem>(item.Id, item.PartitionKey, options, cancellationToken);

    Task PatchItemAsync<TEntity>(TEntity entity, Expression<Func<TEntity, object>> property, object updatedValue, PatchOperationTypeEnum op = PatchOperationTypeEnum.Add) where TEntity : JsonEntity
        => PatchItemAsync(entity, [PatchOperation.Create(property, updatedValue, op)]);

    Task PatchItemAsync<TEntity>(TEntity entity, IList<PatchOperation> patchOperations) where TEntity : JsonEntity
        => PatchItemAsync(entity, (z, pos) => Task.FromResult(patchOperations));

    private static Func<TItem, Task<bool>> CreateAmendAsync<TItem>(Func<TItem, Task> a) where TItem : JsonEntity
    {
        switch (a)
        {
            case null:
                return null;
            default:
                {
                    async Task<bool> amendAsync(TItem item)
                    {
                        var before = item.ToJson();
                        await a(item);
                        var after = item.ToJson();
                        return before != after;
                    }

                    return amendAsync;
                }
        }
    }

    Task<TItem> UpdateItemAsync<TItem>(TItem item, Func<TItem, Task> amendAsync, CancellationToken cancellationToken = default) where TItem : JsonEntity
        => UpdateItemAsync(item, CreateAmendAsync(amendAsync), cancellationToken);

    Task<TItem> UpdateItemAsync<TItem>(TItem item, Action<TItem> amend, CancellationToken cancellationToken = default) where TItem : JsonEntity
        => UpdateItemAsync(item, z => { amend(z); return Task.CompletedTask; }, cancellationToken);

    #endregion
}

