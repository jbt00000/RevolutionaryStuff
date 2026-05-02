using System.IO;
using System.Text.Json;
using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Azure.BackgroundServices;
using RevolutionaryStuff.Azure.Services.Authentication;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.ApplicationNameFinders;

namespace RevolutionaryStuff.Data.Cosmos.BackgroundServices;

public class CosmosChangeFeedBackgroundService<TInboundMessageExecutor, TInboundMessageProcessor> : RevolutionaryStuffBackgroundService
    where TInboundMessageExecutor : class, IInboundMessageExecutor
    where TInboundMessageProcessor : class, IInboundMessageProcessor
{
    private readonly IAzureTokenCredentialProvider AzureTokenCredentialProvider;
    private readonly IConnectionStringProvider ConnectionStringProvider;
    private readonly IOptions<CosmosChangeFeedBackgroundServiceConfig> ConfigOptions;

    public CosmosChangeFeedBackgroundService(IAzureTokenCredentialProvider azureTokenCredentialProvider, IConnectionStringProvider connectionStringProvider, IOptions<CosmosChangeFeedBackgroundServiceConfig> configOptions, RevolutionaryStuffBackgroundServiceConstructorArgs baseConstructorArgs)
    : base(baseConstructorArgs)
    {
        ArgumentNullException.ThrowIfNull(connectionStringProvider);
        ArgumentNullException.ThrowIfNull(configOptions);

        AzureTokenCredentialProvider = azureTokenCredentialProvider;
        ConnectionStringProvider = connectionStringProvider;
        ConfigOptions = configOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = ConfigOptions.Value;
        try
        {
            var executorNames = config
                .Executions
                .NullSafeEnumerable()
                .Select(z => z.Name.TrimOrNull())
                .WhereNotNull()
                .ToList();

            LogWarning("Will execute the following packages: {executorNames}", config.Executions.Where(z => z.Enabled).Select(z => z.Name));

            await Task.WhenAll(config.Executions.Where(z => z.Enabled).Select(z => ExecuteAsync(z.Name, z, stoppingToken)));
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
    }

    private async Task ExecuteAsync(string executionName, CosmosChangeFeedBackgroundServiceConfig.Execution execution, CancellationToken stoppingToken)
    {
        var config = ConfigOptions.Value;

        var connectionString = ConnectionStringProvider.GetConnectionString(execution.ConnectionStringName ?? config.ConnectionStringName);
        var cosmosClient = CosmosHelpers.ConstructCosmosClient(new(connectionString, AzureTokenCredentialProvider, config.AuthenticateWithWithDefaultAzureCredentials), new() { });

        using var _ScopeProperty0 = LogScopedProperty("executionName", executionName);
        using var _ScopeProperty1 = LogScopedProperty("cosmosChangeFeedExecution", execution, true);

        var databaseName = execution.DatabaseName ?? config.DatabaseName;
        var database = cosmosClient.GetDatabase(databaseName);
        var container = database.GetContainer(execution.ContainerName);
        var leaseContainer = database.GetContainer(execution.LeaseContainerName ?? config.LeaseContainerName);

        long docsSeen = 0;
        long errorCount = 0;
        long successCount = 0;

        var builder = container
            .GetChangeFeedProcessorBuilder(executionName, HandleChangesAsync)
            .WithLeaseContainer(leaseContainer);

        var anf = ServiceProvider.GetService<IApplicationNameFinder>();
        if (anf != null)
        {
            builder = builder.WithInstanceName(anf.ApplicationName);
        }
        if (execution.StartTime.HasValue)
        {
            builder = builder.WithStartTime(execution.StartTime.Value);
        }

        var processor = builder.Build();

        await processor.StartAsync();
        await stoppingToken.UntilCancelledAsync();
        await processor.StopAsync();

        async Task HandleChangesAsync(ChangeFeedProcessorContext context, Stream changes, CancellationToken cancellationToken)
        {
            static string GetStringVal(JsonElement jel, string name)
            {
                return jel.TryGetProperty(name, out var el) ? el.GetString() : null;
            }
            using var sr = new StreamReader(changes);
            using var jsonDocument = JsonDocument.Parse(sr.ReadToEnd());
            var positionInBatch = 0;
            foreach (var element in jsonDocument.RootElement.GetProperty("Documents").EnumerateArray())
            {
                ++docsSeen;
                try
                {
                    var id = GetStringVal(element, CosmosEntityPropertyNames.Id);
                    var rid = GetStringVal(element, CosmosEntityPropertyNames.Rid);
                    var self = GetStringVal(element, CosmosEntityPropertyNames.Self);
                    var etag = GetStringVal(element, CosmosEntityPropertyNames.ETag);
                    var touchedAt = DateTimeOffset.UtcNow;
                    if (element.TryGetProperty(CosmosEntityPropertyNames.Timestamp, out var tsElement))
                    {
                        var unixTimestamp = tsElement.GetInt32();
                        touchedAt = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                    }
                    Dictionary<string, object> properties = null;
                    foreach (var kvp in config.DocumentJsonPathToPropertyName.NullSafeEnumerable().Union(execution.DocumentJsonPathToPropertyName.NullSafeEnumerable()))
                    {
                        if (element.TryGetProperty(kvp.Key, out var propElement))
                        {
                            properties ??= [];
                            properties[kvp.Value] = propElement.GetString();
                        }
                    }
                    id = string.Format(config.MessageIdFormat, id, rid, self, etag);
                    var m = new CosmosInboundMessage(id, element, databaseName, execution.ContainerName, docsSeen, touchedAt, properties);

                    using var scope = ServiceProvider.CreateScope();
                    //                    using var loggerScope = CreateLogRegion(LogLevel.Information, $"Processing service bus message on {execution.TopicName}.{execution.SubscriptionName}.{m.SequenceNumber}");
                    var sp = scope.ServiceProvider;
                    var executor = sp.GetRequiredService<TInboundMessageExecutor>();
                    var processor = sp.GetRequiredService<TInboundMessageProcessor>();
                    await executor.ExecuteAsync(m, processor.ProcessInboundMessageAsync);
                    ++successCount;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    ++errorCount;
                }
                LogInformation("{executionName} {successCount}/{errorCount}/{docsSeen} {positionInBatch}", executionName, successCount, errorCount, docsSeen, positionInBatch);
                ++positionInBatch;
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
