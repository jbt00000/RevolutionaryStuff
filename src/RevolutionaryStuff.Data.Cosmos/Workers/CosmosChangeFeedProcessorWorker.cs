using System.IO;
using System.Text.Json;
using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;
using RevolutionaryStuff.Azure.Workers;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.ApplicationNameFinders;
using RevolutionaryStuff.Core.Services.DependencyInjection;

namespace RevolutionaryStuff.Data.Cosmos.Workers;

public class CosmosChangeFeedProcessorWorker : BaseWorker
{
    private readonly IConnectionStringProvider ConnectionStringProvider;
    private readonly IOptions<Config> ConfigOptions;

    public sealed class Config
    {
        public const string ConfigSectionName = "CosmosChangeFeedProcessorWorkerConfig";
        public string ConnectionStringName { get; set; }
        public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;
        public IList<string> ExecutionNames { get; set; }
        public IDictionary<string, Execution> ExecutionByName { get; set; }
        public string LeaseContainerName { get; set; }
        public string DatabaseName { get; set; }
        public IDictionary<string, string> DocumentJsonPathToPropertyName { get; set; }

        public string MessageIdFormat { get; set; } = "{0}/{3}";

        public class Execution
        {
            public DateTime? StartTime { get; set; }
            public string MessageWorkerTypeName { get; set; }
            public string ConnectionStringName { get; set; }
            public string DatabaseName { get; set; }
            public string ContainerName { get; set; }
            public string LeaseContainerName { get; set; }
            public IDictionary<string, string> DocumentJsonPathToPropertyName { get; set; }
        }
    }

    public CosmosChangeFeedProcessorWorker(IConnectionStringProvider connectionStringProvider, IOptions<Config> configOptions, BaseWorkerConstructorArgs baseConstructorArgs, ILogger<CosmosChangeFeedProcessorWorker> logger)
    : base(baseConstructorArgs, logger)
    {
        ArgumentNullException.ThrowIfNull(connectionStringProvider);
        ArgumentNullException.ThrowIfNull(configOptions);

        ConnectionStringProvider = connectionStringProvider;
        ConfigOptions = configOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = ConfigOptions.Value;
        try
        {
            var executorNames = config
                .ExecutionNames
                .NullSafeEnumerable()
                .Select(name => name.TrimOrNull())
                .WhereNotNull()
                .ToList();

            LogWarning("Will execute the following packages: {executorNames}", executorNames);

            await Task.WhenAll(executorNames.Select(name => ExecuteAsync(name, config.ExecutionByName[name], stoppingToken)));
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
    }

    private async Task ExecuteAsync(string executionName, Config.Execution execution, CancellationToken stoppingToken)
    {
        var config = ConfigOptions.Value;

        var connectionString = ConnectionStringProvider.GetConnectionString(execution.ConnectionStringName ?? config.ConnectionStringName);
        var cosmosClient = CosmosHelpers.ConstructCosmosClient(new(connectionString, config.AuthenticateWithWithDefaultAzureCredentials), new() { });

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
                if (jel.TryGetProperty(name, out JsonElement el))
                {
                    return el.GetString();
                }
                return null;
            }
            using var sr = new StreamReader(changes);
            using var jsonDocument = JsonDocument.Parse(sr.ReadToEnd());
            int positionInBatch = 0;
            foreach (JsonElement element in jsonDocument.RootElement.GetProperty("Documents").EnumerateArray())
            {
                ++docsSeen;
                try
                {
                    string id = GetStringVal(element, CosmosEntityPropertyNames.Id);
                    string rid = GetStringVal(element, CosmosEntityPropertyNames.Rid);
                    string self = GetStringVal(element, CosmosEntityPropertyNames.Self);
                    string etag = GetStringVal(element, CosmosEntityPropertyNames.ETag);
                    DateTimeOffset touchedAt = DateTimeOffset.UtcNow;
                    if (element.TryGetProperty(CosmosEntityPropertyNames.Timestamp, out JsonElement tsElement))
                    {
                        int unixTimestamp = tsElement.GetInt32();
                        touchedAt = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                    }
                    Dictionary<string, object> properties = null;
                    foreach (var kvp in config.DocumentJsonPathToPropertyName.NullSafeEnumerable().Union(execution.DocumentJsonPathToPropertyName.NullSafeEnumerable()))
                    {
                        if (element.TryGetProperty(kvp.Key, out JsonElement propElement))
                        {
                            properties ??= new();
                            properties[kvp.Value] = propElement.GetString();
                        }
                    }
                    id = string.Format(config.MessageIdFormat, id, rid, self, etag);
                    var m = (IInboundMessage) new CosmosReceivedMessage(id, element, databaseName, execution.ContainerName, docsSeen, touchedAt, properties);

                    using var scope = ServiceProvider.CreateScope();
//                    using var loggerScope = CreateLogRegion(LogLevel.Information, $"Processing service bus message on {execution.TopicName}.{execution.SubscriptionName}.{m.SequenceNumber}");
                    var sp = scope.ServiceProvider;
                    var executor = sp.GetRequiredService<IInboundMessageExecutor>();
                    var namedFactory = sp.GetRequiredService<INamedFactory>();
                    var processor = namedFactory.GetServiceByName<IInboundMessageProcessor>(execution.MessageWorkerTypeName);
                    await executor.ExecuteAsync(m, processor.ProcessInboundMessageAsync);
                    ++successCount;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    ++errorCount;
                }
                LogInformation("{executionName} {successCount}/{errorCount}/{docsSeen} {positionInBatch}", executionName,  successCount, errorCount, docsSeen, positionInBatch);
                ++positionInBatch;
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
