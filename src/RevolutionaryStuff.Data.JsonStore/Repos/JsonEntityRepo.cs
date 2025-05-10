using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Data.JsonStore.Entities;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Repos;

public abstract class JsonEntityRepo<TBaseEntity> : BaseLoggingDisposable, IJsonEntityRepo<TBaseEntity>
    where TBaseEntity : JsonEntity
{
    protected readonly IJsonEntityRepo<TBaseEntity> I;
    private bool CacherIsScoped;

    private string? TenantId => (this as ITenanted<string>)?.TenantId;

    /// <summary>
    /// This is a cacher that is scoped to the current TenantId
    /// </summary>
    protected ICacher Cacher
    {
        get
        {
            if (field != null && !CacherIsScoped)
            {
                lock (field)
                {
                    if (!CacherIsScoped)
                    {
                        var tid = TenantId;
                        if (tid != null)
                        {
                            field = field.CreateScope(tid);
                        }
                        CacherIsScoped = true;
                    }
                }
            }
            return field;
        }
        private set
        {
            ArgumentNullException.ThrowIfNull(value);
            CacherIsScoped = false;
            field = value;
        }
    }

    protected TimeSpan CacheTimeout => ConfigOptions.Value.CacheTimeout;

    private readonly IOptions<JsonEntityRepoBaseConfig> ConfigOptions;
    protected readonly IJsonEntityServer Jes;
    private readonly IReadOnlyList<string> ContainerIds;

    public IJsonEntityContainer GetContainer<TItem>()
        where TItem : TBaseEntity
    {
        var c = Jes.GetContainer<TItem>();
        Requires.True(ContainerIds.Contains(c.ContainerId));
        return c;
    }

    protected abstract Task<IReadOnlyList<T>> GetAllItemsAsync<T>(IQueryable<T> q, CancellationToken cancellationToken = default) where T : class;

    protected JsonEntityRepo(IList<string> containerIds, JsonEntityRepoConstructorArgs constructorArgs, ILogger logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(constructorArgs);

        I = this;
        Cacher = constructorArgs.Cacher;
        Jes = constructorArgs.Jes;
        ConfigOptions = constructorArgs.ConfigOptions;
        ContainerIds = [.. containerIds];
    }

    protected Task<T> CacheFindOrCreateAsync<T>(bool? force, object cacheKey, Func<Task<T>> createAsync, [CallerMemberName] string memberName = "")
        => CacheFindOrCreateAsync(force, cacheKey == null ? Empty.ObjectArray : [cacheKey], createAsync, memberName);

    protected Task<T> CacheFindOrCreateAsync<T>(bool? force, object cacheKey0, object cacheKey1, Func<Task<T>> createAsync, [CallerMemberName] string memberName = "")
        => CacheFindOrCreateAsync(force, [cacheKey0, cacheKey1], createAsync, memberName);

    protected Task<T> CacheFindOrCreateAsync<T>(RepoCacheRequirements repoCacheRequirements, object cacheKey0, Func<Task<T>> createAsync, [CallerMemberName] string memberName = "")
        => CacheFindOrCreateAsync(repoCacheRequirements, cacheKey0, null, createAsync, memberName);

    protected Task<T> CacheFindOrCreateAsync<T>(RepoCacheRequirements repoCacheRequirements, object cacheKey0, object cacheKey1, Func<Task<T>> createAsync, [CallerMemberName] string memberName = "")
        => CacheFindOrCreateAsync(
            !(repoCacheRequirements ?? RepoCacheRequirements.Default).AllowFetchingOfCachedResults,
            cacheKey0, cacheKey1,
            createAsync,
            memberName);

    protected Task<T> CacheFindOrCreateAsync<T>(bool? force, object[] cacheKeys, Func<Task<T>> createAsync, [CallerMemberName] string memberName = "")
        => Cacher.FindOrCreateValueAsync(
            Cache.CreateKey<T>(new object[] { memberName }.Union(cacheKeys ?? Empty.ObjectArray)),
            createAsync,
            CacheTimeout,
            force.GetValueOrDefault());

    protected static bool IsIdEmpty(string id)
        => string.IsNullOrWhiteSpace(id) || id == "null";

    protected Task<TItem> GetItemByIdAsync<TItem>(string id, string partitionKey, RepoCacheRequirements repoCacheRequirements, CancellationToken cancellationToken = default) where TItem : TBaseEntity
        => GetItemByIdAsync<TItem>(id, partitionKey, !(repoCacheRequirements ?? RepoCacheRequirements.Default).AllowFetchingOfCachedResults, cancellationToken);

    protected async Task<TItem> GetItemByIdAsync<TItem>(string id, string partitionKey = null, bool? force = null, CancellationToken cancellationToken = default) where TItem : TBaseEntity
    {
        //somehow "null" always gets passed in...
        if (IsIdEmpty(id)) return default;

        /*
         * create cache key w/o partition as items (especially of the same type) should NOT have the same id in different partitions
         * but based on calls, sometimes are queried with or without the partition key, and we should try to speed up that subsequent operation
         */

        /*
         * partition key added back in.  Before we can remove this again (which based on the previous comment we should),
         * we need to modify our cacher so we can pass it a parameter telling it (on a call by call basis) to not cache
         * results if the result is default
         */
        var item = await CacheFindOrCreateAsync(force, new[] { id, partitionKey },
            () => GetContainer<TItem>().GetItemByIdAsync<TItem>(id, partitionKey, cancellationToken), nameof(GetItemByIdAsync));

        /*
         * Just in case someone down the line decides to update a member.  
         * This shouldn't break any semantic, as if during a scope, an item was queried, then gets evicted, then queried again, you would not be able to compare instances then either
         */
        return JsonHelpers.Clone(item);
    }

    protected Task<IReadOnlyList<TItem>> GetItemsAsync<TItem>(Expression<Func<TItem, bool>> where, RepoCacheRequirements? repoCacheRequirements = null, CancellationToken cancellationToken = default)
        where TItem : TBaseEntity
        => CacheFindOrCreateAsync(
            repoCacheRequirements,
            Cache.CreateKey<TItem>(nameof(GetItemsAsync), "dynFilter", where?.ToString()),
            () =>
            {
                var q = I.GetQueryable<TItem>();
                if (where != null)
                    q = q.Where(where);
                return GetAllItemsAsync(q, cancellationToken);
            });

    protected Task<IReadOnlyList<TItem>> GetItemsAsync<TItem>(IList<string> keys, RepoCacheRequirements? repoCacheRequirements = null, CancellationToken cancellationToken = default)
        where TItem : TBaseEntity
        => CacheFindOrCreateAsync(
            repoCacheRequirements,
            Cache.CreateKey<TItem>(nameof(GetItemsAsync), keys?.OrderBy()),
            () =>
            {
                //HACKHACK: This semantic of null vs empty array being different will screw us.  Should probably clean up all the api names and make these distinct cases
                if (keys != null && keys.Count == 0)
                    return Task.FromResult<IReadOnlyList<TItem>>([]);
                var q = I.GetQueryable<TItem>();
                if (keys.NullSafeAny())
                    q = q.Where(z => keys.Contains(z.Id));
                return GetAllItemsAsync(q, cancellationToken);
            });

    protected Task CacheItemAsync<TItem>(TItem item) where TItem : TBaseEntity
    {
        if (item == default) return Task.CompletedTask;
        var id = ((IPrimaryKey<string>)item).Key;
        return CacheFindOrCreateAsync(true, id, () => Task.FromResult(item), nameof(GetItemByIdAsync));
    }

    IQueryable<TItem> IJsonEntityRepo<TBaseEntity>.GetQueryable<TItem>(QueryOptions requestOptions)
    {
        var q = GetContainer<TItem>().GetQueryable<TItem>(requestOptions);
        if (typeof(TItem) is ITenanted<string>)
        {
            var methodInfo = GetType().GetMethod(nameof(AppendTenantedQueryableConstraint), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var genericMethod = methodInfo.MakeGenericMethod(typeof(TItem));
            q = (IQueryable<TItem>)genericMethod.Invoke(this, [q]);
        }
        //TODO: NEED TO ADD :: q = q.Where(z => z.SoftDeletedAt == null);
        return q;
    }

    private IQueryable<TItem> AppendTenantedQueryableConstraint<TItem>(IQueryable<TItem> q)
        where TItem : TBaseEntity, ITenanted<string>
    {
        var tid = TenantId;
        if (tid != null)
        {
            q = q.Where(z => z.TenantId == tid);
        }
        return q;
    }

    Task IJsonEntityRepo<TBaseEntity>.CreateItemsAsync<TItem>(IList<TItem> entities)
    {
        if (!entities.NullSafeAny())
            return Task.CompletedTask;
        else if (entities.Count == 1)
        {
            return I.CreateItemAsync(entities[0]);
        }
        else if (ContainerIds.Count == 1)
        {
            var container = GetContainer<TItem>();
            var tasks = entities.Select(z => container.CreateItemAsync(z));
            return Task.WhenAll(tasks);
        }
        else
        {
            Dictionary<Type, IJsonEntityContainer> containerByType = [];
            List<Task> tasks = new(entities.Count);
            foreach (var e in entities)
            {
                var c = containerByType.FindOrCreate(
                    e.GetType(),
                    t => Jes.GetContainer(JsonEntityContainerIdAttribute.GetContainerId(t))
                    );
                tasks.Add(c.CreateItemAsync(e));
            }
            return Task.WhenAll(tasks);
        }
    }

    Task IJsonEntityRepo<TBaseEntity>.CreateItemAsync<TItem>(TItem entity)
        => GetContainer<TItem>().CreateItemAsync(entity);

    Task<TItem> IJsonEntityRepo<TBaseEntity>.UpdateItemAsync<TItem>(TItem entity, Func<TItem, Task> amendAsync)
        => GetContainer<TItem>().UpdateItemAsync(entity, amendAsync);

    Task<TItem> IJsonEntityRepo<TBaseEntity>.UpdateItemAsync<TItem>(TItem entity, Action<TItem> amend)
        => I.UpdateItemAsync(entity, z => { amend(z); return Task.CompletedTask; });

    Task<TItem> IJsonEntityRepo<TBaseEntity>.ReplaceItemAsync<TItem>(TItem item, Func<TItem, Task<TItem>> amendAsync)
        => GetContainer<TItem>().ReplaceItemAsync(item, amendAsync);

    protected void ThrowIfIdInvalid<TItem>(string id)
        where TItem : JsonEntity
        => JsonEntity.JsonEntityIdServices.ThrowIfInvalid<TItem>(id);

    Task IJsonEntityRepo<TBaseEntity>.TouchItemAsync<TItem>(string id, string partitionKey, string? propertyName)
        => GetContainer<TItem>().PatchItemAsync<TItem>(id, partitionKey, [PatchOperation.Add("/" + (propertyName ?? JsonEntity.JsonEntityPropertyNames.TouchedAt), DateTimeOffset.UtcNow.ToIsoString())], null);

    Task<bool> IJsonEntityRepo<TBaseEntity>.PatchItemAsync<TItem>(string id, string partitionKey, Expression<Func<TItem, object>> property, object updatedValue, PatchOperationTypeEnum op, string eTag)
        => GetContainer<TItem>().PatchItemAsync<TItem>(id, partitionKey, [PatchOperation.Create(property, updatedValue, op)], eTag);
}
