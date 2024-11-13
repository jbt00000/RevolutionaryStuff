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

namespace RevolutionaryStuff.Azure.Workers;

public class ServiceBusWorker : BaseWorker
{
    private readonly IConnectionStringProvider ConnectionStringProvider;
    private readonly IOptions<Config> ConfigOptions;

    public class Config : IValidate, IPostConfigure
    {
        public const string ConfigSectionName = "ServiceBusWorkerConfig";

        [Obsolete("Use Executions instead", false)]
        public IList<string> ExecutionNames { get; set; }

        [Obsolete("Use Executions instead", false)]
        public IDictionary<string, Execution> ExecutionByName { get; set; }
        public IList<Execution> Executions { get; set; }
        public string ConnectionStringName { get; set; }
        public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;
        public TimeSpan MessageLockRenewalTimeout { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan MaxMessageLockTime { get; set; } = TimeSpan.FromMinutes(2);
        public TimeSpan RenewalTime { get; set; } = TimeSpan.FromSeconds(10);
        public int MessagePrefetch { get; set; } = 1;
        public int ConcurrentExecutors { get; set; } = 1;

        public void Validate()
            => ExceptionHelpers.AggregateExceptionsAndReThrow(
                () => Requires.Null(ExecutionByName, $"{nameof(ExecutionByName)} is no longer supported, use the Executions list"),
                () => Requires.Null(ExecutionNames, $"{nameof(ExecutionNames)} is no longer supported, use the Executions list"),
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

    public ServiceBusWorker(IConnectionStringProvider connectionStringProvider, IOptions<Config> configOptions, BaseWorkerConstructorArgs baseConstructorArgs, ILogger<ServiceBusWorker> logger)
        : base(baseConstructorArgs, logger)
    {
        ArgumentNullException.ThrowIfNull(connectionStringProvider);
        ArgumentNullException.ThrowIfNull(configOptions);

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
        var concurrentExecutors = execution.ConcurrentExecutors ?? config.ConcurrentExecutors;

        var connectionString = ConnectionStringProvider.GetConnectionString(execution.ConnectionStringName ?? config.ConnectionStringName);
        var serviceBusClient = ServiceBusHelpers.ConstructServiceBusClient(new(connectionString, config.AuthenticateWithWithDefaultAzureCredentials));

        using var _ScopeProperty0 = LogScopedProperty("executionName", executionName);
        using var _ScopeProperty1 = LogScopedProperty("serviceBusExecution", execution, true);

        ServiceBusReceiver listener;
        string listenerPort;

        if (execution.TopicName != null)
        {
            listenerPort = $"{execution.TopicName}.{execution.SubscriptionName}";
            LogWarning(
                "{Host} listening to {listenerPort} running {messageProcessor} with {concurrentExecutors} executors",
                nameof(ServiceBusWorker), listenerPort, execution.MessageWorkerTypeName, concurrentExecutors);

            listener = serviceBusClient.CreateReceiver(execution.TopicName, execution.SubscriptionName, new ServiceBusReceiverOptions
            {
                PrefetchCount = execution.MessagePrefetch ?? config.MessagePrefetch,
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
            });
        }
        else if (execution.QueueName != null)
        {
            listenerPort = $"{execution.QueueName}";
            LogWarning(
                "{Host} listening to {listenerPort} running {messageProcessor} with {concurrentExecutors} executors",
                nameof(ServiceBusWorker), listenerPort, execution.MessageWorkerTypeName, concurrentExecutors);

            listener = serviceBusClient.CreateReceiver(execution.QueueName, new ServiceBusReceiverOptions
            {
                PrefetchCount = execution.MessagePrefetch ?? config.MessagePrefetch,
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
            });
        }
        else
        {
            throw new NotSupportedException("Must either specify a topic or a queue");
        }

        long currentlyRunning = 0;
        long totalRunCount = 0;
        long totalErrorCount = 0;
        long totalSuccessCount = 0;

        async Task executeMessageAsync(ServiceBusReceivedMessage m)
        {
            MessageSupervisorStateBySequenceNumber[m.SequenceNumber] =
                new MessageSupervisorState
                {
                    Listener = listener,
                    Message = m,
                    Execution = execution
                };

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
                Interlocked.Decrement(ref currentlyRunning);
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
                continue;
            _ = executeMessageAsync(message);
        }

        while (currentlyRunning > 0)
        {
            await Task.Delay(1000);
            LogInformation(
                "Shutting down {listenerPort} {running} with {success}/{error}/{total} ...",
                listenerPort, currentlyRunning, totalSuccessCount, totalErrorCount, totalRunCount);
        }

        LogWarning(
            "Shut down {listenerPort} {running} with {success}/{error}/{total}",
            listenerPort, currentlyRunning, totalSuccessCount, totalErrorCount, totalRunCount);
    }
}
