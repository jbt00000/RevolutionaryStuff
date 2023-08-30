using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Data.Cosmos.Services.Setup;

internal class CosmosAdministration : BaseLoggingDisposable, ICosmosAdministration
{
    private readonly ICosmosAdministration I;

    public CosmosAdministration(ILogger<CosmosAdministration> logger)
        : base(logger)
    {
        I = this;
    }

    async Task ICosmosAdministration.SetupContainerAsync(string connectionString, string databaseId, ContainerSetupInfo containerBootstrapInfo)
    {
        Requires.Text(connectionString);
        Requires.Text(databaseId);
        Requires.Valid(containerBootstrapInfo);

        using var cosmos = new CosmosClient(connectionString);

        var dbResp = await cosmos.CreateDatabaseIfNotExistsAsync(databaseId);
        var database = dbResp.Database;

        var containerProperties = new ContainerProperties
        {
            Id = containerBootstrapInfo.ContainerId,
        };
        if (containerBootstrapInfo.PartitionKeyPaths.Count == 1)
        {
            containerProperties.PartitionKeyPath = containerBootstrapInfo.PartitionKeyPaths[0];
        }
        else
        {
            containerProperties.PartitionKeyPaths = containerBootstrapInfo.PartitionKeyPaths;
        }
        if (containerBootstrapInfo.UniqueKeyPaths != null)
        {
            containerProperties.UniqueKeyPolicy = new();
            foreach (var path in containerBootstrapInfo.UniqueKeyPaths)
            {
                var uk = new UniqueKey();
                uk.Paths.Add(path);
                containerProperties.UniqueKeyPolicy.UniqueKeys.Add(uk);
            }
        };
        var containerResp = await database.CreateContainerIfNotExistsAsync(containerProperties);
        var container = containerResp.Container;
        var scripts = container.Scripts;

        if (containerBootstrapInfo.CreateLeasesContainer)
        {
            var leasesContainerId = containerBootstrapInfo.LeasesContainerId ?? "leases";
            await I.SetupContainerAsync(connectionString, databaseId, new()
            {
                ContainerId = leasesContainerId,
                PartitionKeyPaths = new() { "/id" },
                CreateLeasesContainer = false
            });
            const string processorName = $"processorForSetupOfLeasesContainer";
            var changeFeedBuilder = container
                .GetChangeFeedProcessorBuilder(processorName, (leaseContext, leaseChanges, leaseCancellationToken) => {
                    LogDebug("ChangeFeed Processor for {containerId} with {leaseToken}", container.Id, leaseContext.LeaseToken);
                    return Task.CompletedTask;
                })
                .WithPollInterval(TimeSpan.FromSeconds(2))
                .WithLeaseContainer(database.GetContainer(leasesContainerId))
                .WithInstanceName($"processorForSetupOfLeaseContainer");
            var changeFeed = changeFeedBuilder.Build();
            await changeFeed.StartAsync();
            await changeFeed.StopAsync();
        }

        foreach (var sprocInfo in containerBootstrapInfo.StoredProcedureInfos.NullSafeEnumerable())
        {
            var storedProcedureProperties = new StoredProcedureProperties(sprocInfo.StoredProcedureId, sprocInfo.StoredProcedureText);
            await UpsertStoredProcedureAsync(scripts, storedProcedureProperties);
        }
        if (containerBootstrapInfo.DeleteExistingTriggers)
        {
            using var feedIterator = scripts.GetTriggerQueryIterator<TriggerProperties>("select * from t");
            while (feedIterator.HasMoreResults)
            {
                foreach (var properties in await feedIterator.ReadNextAsync())
                {
                    await scripts.DeleteTriggerAsync(properties.Id);
                }
            }
        }
        foreach (var triggerInfo in containerBootstrapInfo.TriggerInfos.NullSafeEnumerable())
        {
            foreach (var triggerType in triggerInfo.TriggerTypes.NullSafeEnumerable())
            {
                foreach (var triggerOperation in triggerInfo.TriggerOperations.NullSafeEnumerable())
                {
                    var triggerProperties = new TriggerProperties
                    {
                        Body = triggerInfo.TriggerText,
                        TriggerType = GetTriggerType(triggerType),
                        TriggerOperation = GetTriggerOperation(triggerOperation),
                        Id = string.Format(triggerInfo.TriggerIdFormat, triggerInfo.TriggerBaseName, triggerType.EnumWithEnumMemberValuesToString(), triggerOperation.EnumWithEnumMemberValuesToString())
                    };
                    await UpsertTriggerAsync(scripts, triggerProperties);
                }
            }
        }
    }

    private static TriggerOperation GetTriggerOperation(TriggerOperationEnum toe)
        => toe switch
        {
            TriggerOperationEnum.Create => TriggerOperation.Create,
            TriggerOperationEnum.Delete => TriggerOperation.Delete,
            TriggerOperationEnum.Replace => TriggerOperation.Replace,
            TriggerOperationEnum.Update => TriggerOperation.Update,
            _ => throw new UnexpectedSwitchValueException(toe)
        };

    private static TriggerType GetTriggerType(TriggerTypeEnum tte)
        => tte switch
        {
            TriggerTypeEnum.Pre => TriggerType.Pre,
            TriggerTypeEnum.Post => TriggerType.Post,
            _ => throw new UnexpectedSwitchValueException(tte)
        };

    private static async Task<StoredProcedureResponse> UpsertStoredProcedureAsync(Scripts scripts, StoredProcedureProperties properties)
    {
        try
        {
            return await scripts.ReplaceStoredProcedureAsync(properties);
        }
        catch (Exception)
        {
            return await scripts.CreateStoredProcedureAsync(properties);
        }
    }

    private static async Task<TriggerResponse> UpsertTriggerAsync(Scripts scripts, TriggerProperties properties)
    {
        try
        {
            return await scripts.ReplaceTriggerAsync(properties);
        }
        catch (Exception)
        {
            return await scripts.CreateTriggerAsync(properties);
        }
    }

}
