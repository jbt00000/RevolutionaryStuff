using System.Threading;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Azure.Services.Authentication;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.DependencyInjection;

namespace RevolutionaryStuff.Azure.BackgroundServices;

public class ServiceBusBackgroundService : RevolutionaryStuffBackgroundService
{
    private readonly IAzureTokenCredentialProvider AzureTokenCredentialProvider;
    private readonly IConnectionStringProvider ConnectionStringProvider;
    private readonly IOptions<Config> ConfigOptions;

    public class Config : IValidate, IPostConfigure
    {
        public const string ConfigSectionName = "ServiceBusWorkerConfig";
        public IList<Execution> Executions { get; set; }
        public string ConnectionStringName { get; set; }
        public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;
        public TimeSpan MaxMessageLockTime { get; set; } = Timeout.InfiniteTimeSpan;
        public int MessagePrefetch { get; set; }
        public int ConcurrentExecutors { get; set; } = 1;

        public void Validate()
            => ExceptionHelpers.AggregateExceptionsAndReThrow(
                () => Executions.ForEach(z => z.Validate())
                );

        void IPostConfigure.PostConfigure()
        {
            Executions ??= [];
            Executions.ForEach(z => z.PostConfigure());
        }

        public class Execution : IValidate, IPostConfigure
        {
            public string Name { get; set; }
            public bool Enabled { get; set; } = true;
            public int? MessagePrefetch { get; set; }
            public int? ConcurrentExecutors { get; set; }
            public string ConnectionStringName { get; set; }
            public TimeSpan? MaxMessageLockTime { get; set; }
            public string QueueName { get; set; }
            public string TopicName { get; set; }
            public string SubscriptionName { get; set; }
            public string MessageWorkerTypeName { get; set; }

            public void Validate()
                => ExceptionHelpers.AggregateExceptionsAndReThrow(
                () => { if (Enabled) Requires.ExactlyOneNonNull(QueueName, TopicName); }
                );

            public void PostConfigure()
                => Name ??= $"{MessageWorkerTypeName} on {QueueName ?? $"{TopicName}.{SubscriptionName}"}";
        }
    }

    public ServiceBusBackgroundService(IAzureTokenCredentialProvider azureTokenCredentialProvider, IConnectionStringProvider connectionStringProvider, IOptions<Config> configOptions, RevolutionaryStuffBackgroundServiceConstructorArgs baseConstructorArgs)
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
        Requires.Valid(config);
        try
        {
            LogWarning("Will execute the following packages: {executorNames}", config.Executions.Where(z => z.Enabled).Select(z => z.Name));

            await Task.WhenAll(config.Executions.Where(z => z.Enabled).Select(z => ExecuteAsync(z.Name, z, stoppingToken)));
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
        await using var serviceBusClient = ServiceBusHelpers.ConstructServiceBusClient(new(connectionString, AzureTokenCredentialProvider, config.AuthenticateWithWithDefaultAzureCredentials));

        using var _ScopeProperty0 = LogScopedProperty("executionName", executionName);
        using var _ScopeProperty1 = LogScopedProperty("serviceBusExecution", execution, true);

        var processorOptions = CreateProcessorOptions(config, execution);
        await using var processor = execution.TopicName != null
            ? serviceBusClient.CreateProcessor(execution.TopicName, execution.SubscriptionName, processorOptions)
            : execution.QueueName != null
                ? serviceBusClient.CreateProcessor(execution.QueueName, processorOptions)
                : throw new NotSupportedException("Must either specify a topic or a queue");
        processor.ProcessMessageAsync += args => ProcessMessageAsync(execution, args);
        processor.ProcessErrorAsync += args =>
        {
            LogError(args.Exception, "Service bus {errorSource} error on {entityPath}", args.ErrorSource, args.EntityPath);
            return Task.CompletedTask;
        };

        LogWarning(
            "{Host} listening to {listenerPort} running {messageProcessor} with {concurrentExecutors} executors",
            nameof(ServiceBusBackgroundService), processor.EntityPath, execution.MessageWorkerTypeName, processorOptions.MaxConcurrentCalls);

        await RunProcessorAsync(processor, stoppingToken);
        LogWarning("Shut down {listenerPort}", processor.EntityPath);
    }

    internal static ServiceBusProcessorOptions CreateProcessorOptions(Config config, Config.Execution execution)
        => new()
        {
            AutoCompleteMessages = true,
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            PrefetchCount = execution.MessagePrefetch ?? config.MessagePrefetch,
            MaxConcurrentCalls = execution.ConcurrentExecutors ?? config.ConcurrentExecutors,
            MaxAutoLockRenewalDuration = execution.MaxMessageLockTime ?? config.MaxMessageLockTime,
        };

    internal static async Task RunProcessorAsync(ServiceBusProcessor processor, CancellationToken stoppingToken)
    {
        try
        {
            await processor.StartProcessingAsync(stoppingToken);
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        { }
        finally
        {
            // Drain handlers before disposing the processor so their locks can still be renewed.
            await processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    internal async Task ProcessMessageAsync(Config.Execution execution, ProcessMessageEventArgs args)
    {
        var message = args.Message;
        using var scope = ServiceProvider.CreateScope();
        using var loggerScope = CreateLogRegion(LogLevel.Information, $"Processing service bus message on {execution.QueueName ?? $"{execution.TopicName}.{execution.SubscriptionName}"}.{message.SequenceNumber}");

        try
        {
            var sp = scope.ServiceProvider;
            var executor = sp.GetRequiredService<IInboundMessageExecutor>();
            var namedFactory = sp.GetRequiredService<INamedFactory>();
            var processor = namedFactory.GetServiceByName<IInboundMessageProcessor>(execution.MessageWorkerTypeName);
            await executor.ExecuteAsync(message, processor.ProcessInboundMessageAsync);
        }
        catch (Exception ex) when (ex is PermanentException or BaseCodedException { IsPermanent: true })
        {
            LogError(ex, "Will dead-letter message {sequenceNumber}", message.SequenceNumber);
            await args.DeadLetterMessageAsync(message);
            return;
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessageLockLost)
        {
            LogWarning("Lock lost for message {sequenceNumber}; it can no longer be settled. {error}", message.SequenceNumber, ex?.Message);
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex);
            await args.AbandonMessageAsync(message);
            return;
        }
    }
}
