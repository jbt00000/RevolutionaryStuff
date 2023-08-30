using System.Linq.Expressions;
using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.Cosmos;
using RevolutionaryStuff.Data.JsonStore.Entities;
using RevolutionaryStuff.Data.JsonStore.Store;
using MAC = Microsoft.Azure.Cosmos;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public class CosmosJsonEntityContainer : BaseLoggingDisposable, ICosmosJsonEntityContainer
{
    private readonly ICosmosJsonEntityContainer I;
    private readonly Container Container;
    private readonly string TenantId;
    private readonly IOptions<CosmosJsonEntityContainerConfig> ConfigOptions;

    public override string ToString()
        => $"{ContainerId}; {base.ToString()}";

    public string ContainerId
        => Container.Id;

    Container ICosmosJsonEntityContainer.Container
        => Container;

    public CosmosJsonEntityContainer(Container container, string tenantId, IOptions<CosmosJsonEntityContainerConfig> configOptions, ILogger<CosmosJsonEntityContainer> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(container);
        Requires.Text(tenantId);

        I = this;
        Container = container;
        TenantId = tenantId;
        ConfigOptions = configOptions;
    }

    private void EnsureCorrectContainer<TItem>()
        where TItem : JsonEntity
    {
        var containerId = JsonEntityContainerIdAttribute.GetContainerId<TItem>();
        if (containerId != I.ContainerId) throw new($"Entity ({typeof(TItem)}) lives container {containerId} but we are trying to operate on container {I.ContainerId}");
    }

    private TItem PrepareItem<TItem>(TItem item)
        where TItem : JsonEntity
    {
        try
        {
            EnsureCorrectContainer<TItem>();
            if (item.TenantId == null)
            {
                item.TenantId = TenantId;
            }
            else if (item.TenantId != TenantId)
            {
                throw new CrossTenantException(item.TenantId, TenantId, item);
            }
            (item as IPreSave)?.PreSave();
            (item as IValidate)?.Validate();
            ArgumentNullException.ThrowIfNull(item.PartitionKey);
        }
        catch (Exception ex)
        {
            LogCritical("PrepareToPersist", ex);
            throw;
        }
        return item;
    }

    private Task<TEntity> ReloadItemAsync<TEntity>(TEntity item, CancellationToken cancellationToken = default)
        where TEntity : JsonEntity
        => I.GetItemByIdAsync<TEntity>(item.Id, item.PartitionKey, cancellationToken);

    private static readonly DeleteOptions DeleteItemDefaultDeleteOptions = new()
    {
        ForceHardDelete = false
    };

    private static readonly PatchItemRequestOptions SoftDeletePatchItemRequestOptions = new()
    {
        EnableContentResponseOnWrite = false
    };

    private static readonly ItemRequestOptions HardDeleteItemRequestOptions = new()
    {
        EnableContentResponseOnWrite = false
    };

    Task IJsonEntityContainer.DeleteItemAsync<TItem>(string id, string partitionKey, DeleteOptions options, CancellationToken cancellationToken)
    {
        JsonEntity.JsonEntityIdServices.ThrowIfInvalid(typeof(TItem), id);
        Requires.Text(partitionKey);

        options ??= DeleteItemDefaultDeleteOptions;
        Requires.Valid(options);

        return options.ForceHardDelete
            ? Container.DeleteItemAsync<TItem>(
                id,
                new PartitionKey(partitionKey),
                HardDeleteItemRequestOptions,
                cancellationToken)
            : (Task)Container.PatchItemAsync<TItem>(
                id,
                new PartitionKey(partitionKey),
                new[] { MAC.PatchOperation.Set("/" + JsonEntity.JsonEntityPropertyNames.SoftDeletedAt, DateTimeOffset.Now.ToIsoString()) },
                SoftDeletePatchItemRequestOptions,
                cancellationToken);
    }

    private static readonly ItemRequestOptions CreateItemDefaultItemRequestOptions = new()
    {
        EnableContentResponseOnWrite = false
    };

    Task IJsonEntityContainer.CreateItemAsync<TItem>(TItem item, CancellationToken cancellationToken)
        => Container.CreateItemAsync(PrepareItem(item), new PartitionKey(item.PartitionKey), CreateItemDefaultItemRequestOptions, cancellationToken);


    /// <summary>
    /// Overrride so you can do things such as setting a default integrated gateway cache settings for the application
    /// </summary>
    /// <param name="options">ItemRequestOptions for override configuration that will be used in an upcoming ReadItem call</param>
    protected virtual void ConfigureItemRequestOptions(ItemRequestOptions options)
    {
        //TODO: Add suport for default settings into CosmosJsonEntityContainerConfig and then set during the ConfigureQueryRequestOptions and ConfigureItemRequestOptions methods
    }

    async Task<TItem> IJsonEntityContainer.GetItemByIdAsync<TItem>(string id, string partitionKey, CancellationToken cancellationToken)
    {
        JsonEntity.JsonEntityIdServices.ThrowIfInvalid(typeof(TItem), id);

        if (string.IsNullOrEmpty(partitionKey))
        {
            var q = I.GetQueryable<TItem>(partitionKey == null ? null : QueryOptions.CreateWithParitionKey(partitionKey));
            return await q.Where(z => z.Id == id).GetFirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            var options = new ItemRequestOptions();
            ConfigureItemRequestOptions(options);
            var resp = await Container.ReadItemAsync<TItem>(id, new PartitionKey(partitionKey), options, cancellationToken);
            return resp.Resource;
        }
    }

    private static readonly QueryOptions GetQueryableDefaultQueryOptions = new()
    { };

    private QueryRequestOptions CreateQueryRequestOptions(QueryOptions options)
    {
        var ro = new QueryRequestOptions();
        if (options.PartitionKey != null)
        {
            ro.PartitionKey = new PartitionKey(options.PartitionKey);
        }
        ConfigureQueryRequestOptions(ro);
        return ro;
    }

    /// <summary>
    /// Overrride so you can do things such as setting a default integrated gateway cache settings for the application
    /// </summary>
    /// <param name="options">QueryRequestOptions to be overridden at an application level for an upcoming Query operation</param>
    protected virtual void ConfigureQueryRequestOptions(QueryRequestOptions options)
    { }

    IQueryable<TItem> IJsonEntityContainer.GetQueryable<TItem>(QueryOptions queryOptions)
    {
        EnsureCorrectContainer<TItem>();
        queryOptions ??= GetQueryableDefaultQueryOptions;
        Requires.Valid(queryOptions);

        var q = (IQueryable<TItem>)Container.GetItemLinqQueryable<TItem>(requestOptions: CreateQueryRequestOptions(queryOptions));

        q = q.Where(z => z.TenantId == TenantId && (!z.SoftDeletedAt.IsDefined() || z.SoftDeletedAt == null));

        if (!queryOptions.IgnoreEntityDataType)
        {
            var dt = JsonEntity.GetDataType<TItem>();
            q = q.Where(z => z.DataType == dt);
        }

        if (queryOptions?.PartitionKey != null)
        {
            q = q.Where(z => z.PartitionKey == queryOptions.PartitionKey);
        }

        return q;
    }

    Task<IReadOnlyList<TItem>> IJsonEntityContainer.GetItemsAsync<TItem>(Expression<Func<TItem, bool>> predicate, QueryOptions options, CancellationToken cancellationToken)
    {
        var q = I.GetQueryable<TItem>(options);
        if (predicate != null)
        {
            q = q.Where(predicate);
        }
        return q.GetAllItemsAsync(cancellationToken);
    }

    private static MAC.PatchOperation CreatePatchOperation(Store.PatchOperation po)
        => po.PatchOperationType switch
        {
            Store.PatchOperationTypeEnum.Add => MAC.PatchOperation.Add(po.Path, po.Value),
            Store.PatchOperationTypeEnum.Replace => MAC.PatchOperation.Replace(po.Path, po.Value),
            _ => throw new UnexpectedSwitchValueException(po.PatchOperationType)
        };

    async Task IJsonEntityContainer.PatchItemAsync<TItem>(TItem item, Func<TItem, CancellationToken, Task<IList<Store.PatchOperation>>> getPatchesAsync, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(getPatchesAsync);
        EnsureCorrectContainer<TItem>();

        item = string.IsNullOrWhiteSpace(item.ETag) ? await ReloadItemAsync(item, cancellationToken) : item;
        await CosmosRetryItemRefreshFunctionAsync(
            item,
            async (z, token) => await Container.PatchItemAsync<TItem>(
                z.Id,
                new PartitionKey(z.PartitionKey),
                (await getPatchesAsync(z, token)).Select(CreatePatchOperation).ToList().AsReadOnly(),
                new PatchItemRequestOptions
                {
                    IfMatchEtag = z.ETag
                },
                token),
            (z, token) => ReloadItemAsync(z, token),
            ConfigOptions.Value.PreconditionFailedRetryInfo,
            cancellationToken);
    }

    async Task<TItem> IJsonEntityContainer.UpdateItemAsync<TItem>(TItem item, Func<TItem, Task<bool>> amendAsync, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(amendAsync);
        EnsureCorrectContainer<TItem>();

        item = string.IsNullOrWhiteSpace(item.ETag) ? await ReloadItemAsync(item, cancellationToken) : item;
        Requires.Text(item.ETag);

        return await CosmosRetryItemRefreshFunctionAsync(
            item,
            async (z, token) =>
            {
                var update = await amendAsync(z);
                if (!update) return;
                await Container.UpsertItemAsync<TItem>(
                    z,
                    new PartitionKey(z.PartitionKey),
                    new ItemRequestOptions
                    {
                        EnableContentResponseOnWrite = false,
                        IfMatchEtag = z.ETag
                    },
                    token);
            },
            (z, token) => ReloadItemAsync(z, token),
            ConfigOptions.Value.PreconditionFailedRetryInfo,
            cancellationToken);
    }

    private static async Task<TItem> CosmosRetryItemRefreshFunctionAsync<TItem>(
        TItem item,
        Func<TItem, CancellationToken, Task> executeAsync,
        Func<TItem, CancellationToken, Task<TItem>> reloadAsync,
        CosmosJsonEntityContainerConfig.RetryInfo retryInfo,
        CancellationToken cancellationToken
        )
    where TItem : JsonEntity
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(retryInfo.DelayBetweenRetries, retryInfo.MaxRetries);
        var policy = Policy.Handle<CosmosException>(ex => ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
            .WaitAndRetryAsync(delay, onRetry: async (_, _) =>
            {
                // Add logic to be executed before each retry
                if (reloadAsync != null)
                {
                    item = await reloadAsync(item, cancellationToken);
                }
            });

        var result = await policy.ExecuteAndCaptureAsync(
            ct => executeAsync.Invoke(item, cancellationToken),
            cancellationToken
        );
        return result.Outcome == OutcomeType.Successful
            ? item
            : throw new($"Retry count ({retryInfo}) exceeded. Final outcome {result.Outcome}");
    }
}
