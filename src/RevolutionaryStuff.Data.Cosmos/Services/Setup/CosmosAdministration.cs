using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Data.Cosmos.Services.Setup;

internal class CosmosAdministration : BaseLoggingDisposable, ICosmosAdministration
{
    public CosmosAdministration(ILogger<CosmosAdministration> logger)
        : base(logger)
    { }

    async Task ICosmosAdministration.SetupContainerAsync(string connectionString, string databaseId, ContainerSetupInfo containerBootstrapInfo)
    {
        Requires.Text(connectionString);
        Requires.Text(databaseId);
        ArgumentNullException.ThrowIfNull(containerBootstrapInfo);

        using var cosmos = new CosmosClient(connectionString);

        var dbResp = await cosmos.CreateDatabaseIfNotExistsAsync(databaseId);
        var containerProperties = new ContainerProperties
        {
            Id = containerBootstrapInfo.ContainerId,
            PartitionKeyPath = containerBootstrapInfo.PartitionKeyPath
        };
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
        var containerResp = await dbResp.Database.CreateContainerIfNotExistsAsync(containerProperties);
        var scripts = containerResp.Container.Scripts;
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
