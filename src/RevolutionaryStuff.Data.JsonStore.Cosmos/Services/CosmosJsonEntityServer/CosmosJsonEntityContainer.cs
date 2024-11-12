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

public class CosmosJsonEntityContainer : BaseLoggingDisposable, ICosmosJsonEntityContainer, ITenanted<string>
{
    private readonly ICosmosJsonEntityContainer I;
    protected readonly Container Container;
    protected readonly string TenantId;
    private readonly IOptions<CosmosJsonEntityServerConfig> ServerConfigOptions;

    public override string ToString()
        => $"{ContainerId}; {base.ToString()}";

    public string ContainerId
        => Container.Id;

    Container ICosmosJsonEntityContainer.Container
        => Container;

    string ITenanted<string>.TenantId
    {
        get => TenantId;
        set => throw new NotSupportedException();
    }

    protected CosmosJsonEntityServerConfig.ContainerConfig ContainerConfig
        => ServerConfigOptions.Value.ContainerConfigByContainerKey.GetValueOrDefault(ContainerId);

    public CosmosJsonEntityContainer(Container container, string tenantId, IOptions<CosmosJsonEntityServerConfig> serverConfigOptions, ILogger logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(container);
        Requires.Text(tenantId);

        I = this;
        Container = container;
        TenantId = tenantId;
        ServerConfigOptions = serverConfigOptions;
    }

    private void LogOperationRequestCharge(CosmosOperationEnum operation, double requestCharge)
    {
        if (ServerConfigOptions.Value.EnableAnalytics)
        {
            var rcl = ServerConfigOptions.Value.RequestChargeLoggings.Where(z => z.Operation == operation && z.RequestCharge <= requestCharge).MaxBy(z => z.RequestCharge);
            var level = rcl?.LogLevel ?? LogLevel.Trace;
            Log(level, nameof(CosmosJsonEntityContainer) + " operation {operation} on {containerId} cost {requestUnits} RU", operation, ContainerId, requestCharge);
        }
    }

    private void EnsureCorrectContainer<TItem>()
        where TItem : JsonEntity
    {
        var containerId = JsonEntityContainerIdAttribute.GetContainerId<TItem>();
        if (containerId != I.ContainerId) throw new($"Entity ({typeof(TItem)}) lives container {containerId} but we are trying to operate on container {I.ContainerId}");
    }

    protected virtual TItem PrepareItem<TItem>(TItem item)
        where TItem : JsonEntity
    {
        try
        {
            EnsureCorrectContainer<TItem>();
            (item as IPreSave)?.PreSave();
            (item as JsonEntity)?.PreSave(this);
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

    //TODO: CreatePartitionKey to inspect the base container entity, looking for any multi-part keys that have implicit parameters (like tenantId), and construct a full key based on this 
    protected virtual PartitionKey CreatePartitionKey(string partitionKey)
        => new(partitionKey);

    async Task IJsonEntityContainer.DeleteItemAsync<TItem>(string id, string partitionKey, DeleteOptions options, CancellationToken cancellationToken)
    {
        JsonEntity.JsonEntityIdServices.ThrowIfInvalid(typeof(TItem), id);
        Requires.Text(partitionKey);

        options ??= DeleteItemDefaultDeleteOptions;
        Requires.Valid(options);

        if (options.ForceHardDelete)
        {
            var resp = await Container.DeleteItemAsync<TItem>(
                id,
                CreatePartitionKey(partitionKey),
                HardDeleteItemRequestOptions,
                cancellationToken);
            LogOperationRequestCharge(CosmosOperationEnum.DeleteHard, resp.RequestCharge);
        }
        else
        {
            var resp = await Container.PatchItemAsync<TItem>(
                id,
                CreatePartitionKey(partitionKey),
                new[] { MAC.PatchOperation.Set("/" + JsonEntity.JsonEntityPropertyNames.SoftDeletedAt, DateTimeOffset.Now.ToIsoString()) },
                SoftDeletePatchItemRequestOptions,
                cancellationToken);
            LogOperationRequestCharge(CosmosOperationEnum.DeleteSoft, resp.RequestCharge);
        }
    }

    private static readonly ItemRequestOptions CreateItemDefaultItemRequestOptions = new()
    {
        EnableContentResponseOnWrite = false
    };

    async Task IJsonEntityContainer.CreateItemAsync<TItem>(TItem item, CancellationToken cancellationToken)
    {
        var resp = await Container.CreateItemAsync(PrepareItem(item), CreatePartitionKey(item.PartitionKey), CreateItemDefaultItemRequestOptions, cancellationToken);
        LogOperationRequestCharge(CosmosOperationEnum.Create, resp.RequestCharge);
    }


    /// <summary>
    /// Override so you can do things such as setting a default integrated gateway cache settings for the application
    /// </summary>
    /// <param name="options">ItemRequestOptions for override configuration that will be used in an upcoming ReadItem call</param>
    protected virtual void ConfigureItemRequestOptions(ItemRequestOptions options)
    {
        //TODO: Add support for default settings into CosmosJsonEntityContainerConfig and then set during the ConfigureQueryRequestOptions and ConfigureItemRequestOptions methods
    }

    private ItemRequestOptions GetItemByIdItemRequestOptions;

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
            var options = GetItemByIdItemRequestOptions;
            if (options == null)
            {
                options = new ItemRequestOptions();
                ConfigureItemRequestOptions(options);
                GetItemByIdItemRequestOptions = options;
            }
            try
            {
                var resp = await Container.ReadItemAsync<TItem>(id, CreatePartitionKey(partitionKey), options, cancellationToken);
                LogOperationRequestCharge(CosmosOperationEnum.Read, resp.RequestCharge);
                return resp.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                LogOperationRequestCharge(CosmosOperationEnum.Read, ex.RequestCharge);
                return default;
            }
        }
    }

    private static readonly QueryOptions GetQueryableDefaultQueryOptions = new()
    { };

    private QueryRequestOptions CreateQueryRequestOptions(QueryOptions options)
    {
        var ro = new QueryRequestOptions();
        if (options.PartitionKey != null)
        {
            ro.PartitionKey = CreatePartitionKey(options.PartitionKey);
        }
        ConfigureQueryRequestOptions(ro);
        return ro;
    }

    /// <summary>
    /// Override so you can do things such as setting a default integrated gateway cache settings for the application
    /// </summary>
    /// <param name="options">QueryRequestOptions to be overridden at an application level for an upcoming Query operation</param>
    protected virtual void ConfigureQueryRequestOptions(QueryRequestOptions options)
    { }

    protected virtual IQueryable<TItem> ConfigureQueryable<TItem>(IQueryable<TItem> q, QueryOptions queryOptions) where TItem : JsonEntity
    {
        q = q.Where(z => !z.SoftDeletedAt.IsDefined() || z.SoftDeletedAt == null);

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

    protected virtual IQueryable<TItem> CreateQueryable<TItem>(QueryRequestOptions queryRequestOptions) where TItem : JsonEntity
        => Container.GetItemLinqQueryable<TItem>(requestOptions: queryRequestOptions);

    IQueryable<TItem> IJsonEntityContainer.GetQueryable<TItem>(QueryOptions queryOptions)
    {
        EnsureCorrectContainer<TItem>();
        queryOptions ??= GetQueryableDefaultQueryOptions;
        Requires.Valid(queryOptions);

        var q = CreateQueryable<TItem>(CreateQueryRequestOptions(queryOptions));

        q = ConfigureQueryable(q, queryOptions);

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
        var resp = await CosmosRetryItemRefreshFunctionAsync(
            item,
            async (z, token) => await Container.PatchItemAsync<TItem>(
                z.Id,
                CreatePartitionKey(z.PartitionKey),
                (await getPatchesAsync(z, token)).Select(CreatePatchOperation).ToList().AsReadOnly(),
                new PatchItemRequestOptions
                {
                    IfMatchEtag = z.ETag
                },
                token),
            (z, token) => ReloadItemAsync(z, token),
            ServerConfigOptions.Value.PreconditionFailedRetryInfo,
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
                PrepareItem(z);
                await Container.UpsertItemAsync<TItem>(
                    z,
                    CreatePartitionKey(z.PartitionKey),
                    new ItemRequestOptions
                    {
                        EnableContentResponseOnWrite = false,
                        IfMatchEtag = z.ETag
                    },
                    token);
            },
            (z, token) => ReloadItemAsync(z, token),
            ServerConfigOptions.Value.PreconditionFailedRetryInfo,
            cancellationToken);
    }

    async Task<TItem> IJsonEntityContainer.ReplaceItemAsync<TItem>(TItem item, Func<TItem, Task<TItem>> amendAsync, CancellationToken cancellationToken)
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
                var replacement = await amendAsync(z);
                Requires.AreEqual(item.Id, replacement.Id);
                Requires.AreEqual(item.PartitionKey, replacement.PartitionKey);
                await Container.ReplaceItemAsync<TItem>(
                    replacement,
                    replacement.Id,
                    CreatePartitionKey(replacement.PartitionKey),
                    new ItemRequestOptions
                    {
                        EnableContentResponseOnWrite = false,
                        IfMatchEtag = z.ETag
                    },
                    token);
            },
            (z, token) => ReloadItemAsync(z, token),
            ServerConfigOptions.Value.PreconditionFailedRetryInfo,
            cancellationToken);
    }

    private static async Task<TItem> CosmosRetryItemRefreshFunctionAsync<TItem>(
        TItem item,
        Func<TItem, CancellationToken, Task> executeAsync,
        Func<TItem, CancellationToken, Task<TItem>> reloadAsync,
        CosmosJsonEntityServerConfig.RetryInfo retryInfo,
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
