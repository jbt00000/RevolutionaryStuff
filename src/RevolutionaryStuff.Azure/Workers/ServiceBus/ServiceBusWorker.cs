using System.Collections.Concurrent;
using System.Threading;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.DependencyInjection;
using RevolutionaryStuff.Core.Threading;

namespace RevolutionaryStuff.Azure.Workers.ServiceBus;

public class ServiceBusWorker : BaseWorker
{
    private readonly IServiceProvider ServiceProvider;
    private readonly IConnectionStringProvider ConnectionStringProvider;
    private readonly IOptions<Config> ConfigOptions;

    public class Config
    {
        public const string ConfigSectionName = "ServiceBusWorkerConfig";
        public IList<string> ExecutionNames { get; set; }
        public IDictionary<string, Execution> ExecutionByName { get; set; }
        public string ConnectionStringName { get; set; }
        public TimeSpan MessageLockRenewalTimeout { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan MaxMessageLockTime { get; set; } = TimeSpan.FromMinutes(2);
        public TimeSpan RenewalTime { get; set; } = TimeSpan.FromSeconds(10);
        public int MessagePrefetch { get; set; } = 1;
        public int ConcurrentExecutors { get; set; } = 1;

        public class Execution
        {
            public int? MessagePrefetch { get; set; }
            public int? ConcurrentExecutors { get; set; }
            public string ConnectionStringName { get; set; }
            public TimeSpan? MaxMessageLockTime { get; set; }
            public string TopicName { get; set; }
            public string SubscriptionName { get; set; }
            public string MessageWorkerTypeName { get; set; }
        }
    }

    private readonly IDictionary<long, MessageSupervisorState> MessageSupervisorStateBySequenceNumber = new ConcurrentDictionary<long, MessageSupervisorState>();

    private class MessageSupervisorState
    {
        public readonly DateTimeOffset StartedAt = DateTimeOffset.UtcNow;
        public DateTimeOffset RenewedAt = DateTimeOffset.UtcNow;
        public int RenewCount = 0;
        public bool Abandoned;
        public Config.Execution Execution { get; init; }
        public ServiceBusReceiver Listener { get; init; }
        public ServiceBusReceivedMessage Message { get; init; }
    }

    public ServiceBusWorker(IServiceProvider serviceProvider, IConnectionStringProvider connectionStringProvider, IOptions<Config> configOptions, ILogger<ServiceBusWorker> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(connectionStringProvider);
        ArgumentNullException.ThrowIfNull(configOptions);
        ArgumentNullException.ThrowIfNull(logger);

        ServiceProvider = serviceProvider;
        ConnectionStringProvider = connectionStringProvider;
        ConfigOptions = configOptions;
    }

    private static readonly TimeSpan SupervisorTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ListenerTimeout = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = ConfigOptions.Value;
        using var pa = new PeriodicAction(async () =>
        {
            if (MessageSupervisorStateBySequenceNumber.Count == 0) return;
            var states = MessageSupervisorStateBySequenceNumber.Values.Where(z => !z.Abandoned).ToList();
            if (states.Count == 0) return;
            foreach (var mss in states)
            {
                var now = DateTimeOffset.UtcNow;
                var maxMessageLockTime = mss.Execution.MaxMessageLockTime.GetValueOrDefault(config.MaxMessageLockTime);
                if (now.Subtract(mss.StartedAt) > maxMessageLockTime)
                {
                    mss.Abandoned = true;
                    LogWarning(
                        "Message {sequenceId} started at {messageFirsSeenAt} and had been executing longer than {maxMessageLockTime}. Will NOT renew service bus message lock.",
                        mss.Message.SequenceNumber,
                        mss.StartedAt,
                        maxMessageLockTime);
                }
                else if (now.Subtract(mss.RenewedAt) > config.RenewalTime)
                {
                    try
                    {
                        await mss.Listener.RenewMessageLockAsync(mss.Message, stoppingToken);
                        mss.RenewedAt = DateTimeOffset.Now;
                        mss.RenewCount += 1;
                        LogWarning(
                            "Message {sequenceId} started at {messageFirsSeenAt} and we just renewed the message lock.",
                            mss.Message.SequenceNumber,
                            mss.StartedAt);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Problem renewing service bus lease for message {sequenceId}", mss.Message.SequenceNumber);
                    }
                }
            }
        }, SupervisorTimeout, SupervisorTimeout);
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
        var concurrentExecutors = execution.ConcurrentExecutors ?? config.ConcurrentExecutors;

        var connectionString = ConnectionStringProvider.GetConnectionString(execution.ConnectionStringName ?? config.ConnectionStringName);
        var serviceBusClient = new ServiceBusClient(connectionString);

        using var _ScopeProperty0 = LogScopedProperty("{executionName}", executionName);
        using var _ScopeProperty1 = LogScopedProperty("{@ServiceBusExecution}", execution);

        LogWarning(
            "{Host} listening to {topic}.{subscription} running {messageProcessor} with {concurrentExecutors} executors",
            nameof(ServiceBusWorker), execution.TopicName, execution.SubscriptionName, execution.MessageWorkerTypeName, concurrentExecutors);

        var listener = serviceBusClient.CreateReceiver(execution.TopicName, execution.SubscriptionName, new ServiceBusReceiverOptions
        {
            PrefetchCount = execution.MessagePrefetch ?? config.MessagePrefetch,             
        });

        long currentlyRunning = 0;
        long totalRunCount = 0;
        long totalErrorCount = 0;
        long totalSuccessCount = 0;

        async Task executeMessageAsync(ServiceBusReceivedMessage m)
        {
            MessageSupervisorStateBySequenceNumber.Add(
                m.SequenceNumber,
                new MessageSupervisorState
                {
                    Listener = listener,
                    Message = m,
                    Execution = execution
                });

            using var scope = ServiceProvider.CreateScope();
            using var loggerScope = CreateLogRegion(LogLevel.Information, $"Processing service bus message on {execution.TopicName}.{execution.SubscriptionName}.{m.SequenceNumber}");

            Interlocked.Increment(ref currentlyRunning);
            Interlocked.Increment(ref totalRunCount);

            try
            {
                var sp = scope.ServiceProvider;
                var executor = sp.GetRequiredService<IInboundMessageExecutor>();
                var namedFactory = sp.GetRequiredService<INamedFactory>();
                var processor = namedFactory.GetServiceByName<IInboundMessageProcessor>(execution.MessageWorkerTypeName);
                await executor.ExecuteAsync(m, processor.ProcessInboundMessageAsync);

                await listener.CompleteMessageAsync(m);
                Interlocked.Increment(ref totalSuccessCount);
            }
            catch (PermanentException ex)
            {
                LogError(ex, "Will abandon message {sequenceNumber}", m.SequenceNumber);
                await listener.DeadLetterMessageAsync(m);
                Interlocked.Increment(ref totalErrorCount);
            }
            catch (BaseCodedException ex) when (ex.IsPermanent)
            {
                LogError(ex, "Will abandon message {sequenceNumber}", m.SequenceNumber);
                await listener.DeadLetterMessageAsync(m);
                Interlocked.Increment(ref totalErrorCount);
            }
            catch (Exception ex)
            {
                LogError(ex);
                await listener.AbandonMessageAsync(m);
                Interlocked.Increment(ref totalErrorCount);
            }
            finally
            {
                Interlocked.Decrement(ref totalRunCount);
                MessageSupervisorStateBySequenceNumber.Remove(m.SequenceNumber);
            }
        }

        for (; !stoppingToken.IsCancellationRequested;)
        {
            if (Interlocked.Read(ref currentlyRunning) > concurrentExecutors)
            {
                await Task.Delay(100);
                continue;
            }
            var message = await listener.ReceiveMessageAsync(ListenerTimeout, stoppingToken);
            if (message == null)
            {
                continue;
            }
            _ = executeMessageAsync(message);
        }

        while (currentlyRunning > 0)
        {
            await Task.Delay(1000);
            LogInformation(
                "Shutting down {topicName}.{subscriptionName} {running} with {success}/{error}/{total} ...",
                execution.TopicName, execution.SubscriptionName, currentlyRunning, totalSuccessCount, totalErrorCount, totalRunCount);
        }

        LogWarning(
            "Shut down {topicName}.{subscriptionName} {running} with {success}/{error}/{total}",
            execution.TopicName, execution.SubscriptionName, currentlyRunning, totalSuccessCount, totalErrorCount, totalRunCount);
    }
}
