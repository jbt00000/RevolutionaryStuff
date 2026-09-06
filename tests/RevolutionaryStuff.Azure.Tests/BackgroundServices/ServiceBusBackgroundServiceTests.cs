using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Azure.BackgroundServices;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.DependencyInjection;

namespace RevolutionaryStuff.Azure.Tests.BackgroundServices;

[TestClass]
public class ServiceBusBackgroundServiceTests
{
    #region Processor Options

    [TestMethod]
    public void CreateProcessorOptions_DefaultsKeepActiveMessagesLocked()
    {
        var options = ServiceBusBackgroundService.CreateProcessorOptions(new(), new());

        Assert.AreEqual(ServiceBusReceiveMode.PeekLock, options.ReceiveMode);
        Assert.IsFalse(options.AutoCompleteMessages);
        Assert.AreEqual(Timeout.InfiniteTimeSpan, options.MaxAutoLockRenewalDuration);
        Assert.AreEqual(0, options.PrefetchCount);
        Assert.AreEqual(1, options.MaxConcurrentCalls);
    }

    [TestMethod]
    public void CreateProcessorOptions_UsesGlobalSettings()
    {
        var config = new ServiceBusBackgroundService.Config
        {
            MaxMessageLockTime = TimeSpan.FromMinutes(30),
            MessagePrefetch = 4,
            ConcurrentExecutors = 3,
        };

        var options = ServiceBusBackgroundService.CreateProcessorOptions(config, new());

        Assert.AreEqual(config.MaxMessageLockTime, options.MaxAutoLockRenewalDuration);
        Assert.AreEqual(config.MessagePrefetch, options.PrefetchCount);
        Assert.AreEqual(config.ConcurrentExecutors, options.MaxConcurrentCalls);
    }

    [TestMethod]
    public void CreateProcessorOptions_ExecutionSettingsOverrideGlobalSettings()
    {
        var config = new ServiceBusBackgroundService.Config
        {
            MaxMessageLockTime = TimeSpan.FromMinutes(2),
            MessagePrefetch = 10,
            ConcurrentExecutors = 3,
        };
        var execution = new ServiceBusBackgroundService.Config.Execution
        {
            MaxMessageLockTime = Timeout.InfiniteTimeSpan,
            MessagePrefetch = 0,
            ConcurrentExecutors = 2,
        };

        var options = ServiceBusBackgroundService.CreateProcessorOptions(config, execution);

        Assert.AreEqual(Timeout.InfiniteTimeSpan, options.MaxAutoLockRenewalDuration);
        Assert.AreEqual(0, options.PrefetchCount);
        Assert.AreEqual(2, options.MaxConcurrentCalls);
    }

    #endregion

    #region Processor Lifecycle

    [TestMethod]
    public async Task RunProcessorAsync_CancellationWaitsForHandlersToDrainAsync()
    {
        using var cancellation = new CancellationTokenSource();
        await using var processor = new TestServiceBusProcessor();
        var runTask = ServiceBusBackgroundService.RunProcessorAsync(processor, cancellation.Token);
        await processor.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsFalse(runTask.IsCompleted);

        cancellation.Cancel();
        try
        {
            await processor.Stopping.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.IsFalse(processor.StopToken.CanBeCanceled);
            Assert.IsFalse(runTask.IsCompleted);
        }
        finally
        {
            processor.Drained.TrySetResult();
            await runTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [TestMethod]
    public async Task RunProcessorAsync_AlreadyCanceledStopsCleanlyAsync()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await using var processor = new TestServiceBusProcessor();
        processor.Drained.SetResult();

        await ServiceBusBackgroundService.RunProcessorAsync(processor, cancellation.Token);

        Assert.IsTrue(processor.Stopping.Task.IsCompleted);
        Assert.IsFalse(processor.StopToken.CanBeCanceled);
    }

    [TestMethod]
    public async Task RunProcessorAsync_StartupFailureStillStopsProcessorAsync()
    {
        var failure = new InvalidOperationException("Startup failed");
        await using var processor = new TestServiceBusProcessor { StartFailure = failure };
        processor.Drained.SetResult();

        var actual = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => ServiceBusBackgroundService.RunProcessorAsync(processor, CancellationToken.None));

        Assert.AreSame(failure, actual);
        Assert.IsTrue(processor.Stopping.Task.IsCompleted);
    }

    #endregion

    #region Message Settlement

    [TestMethod]
    public async Task ProcessMessageAsync_CompletesOnlyAfterHandlerFinishesAsync()
    {
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var context = new MessageTestContext(() => finished.Task);
        var processingTask = context.ProcessAsync();

        Assert.IsFalse(processingTask.IsCompleted);
        Assert.HasCount(0, context.Receiver.Settlements);
        finished.SetResult();
        await processingTask.WaitAsync(TimeSpan.FromSeconds(5));

        CollectionAssert.AreEqual(new[] { "complete" }, context.Receiver.Settlements);
        Assert.IsTrue(context.Processors[0].Disposed);
        Assert.AreEqual("worker", context.ResolvedName);
    }

    [TestMethod]
    public async Task ProcessMessageAsync_CreatesAndDisposesScopeForEachMessageAsync()
    {
        using var context = new MessageTestContext(() => Task.CompletedTask);

        await context.ProcessAsync();
        await context.ProcessAsync();

        Assert.HasCount(2, context.Processors);
        Assert.AreNotSame(context.Processors[0], context.Processors[1]);
        Assert.IsTrue(context.Processors.TrueForAll(z => z.Disposed));
        CollectionAssert.AreEqual(new[] { "complete", "complete" }, context.Receiver.Settlements);
    }

    [TestMethod]
    public async Task ProcessMessageAsync_PermanentFailureDeadLettersAsync()
    {
        using var context = new MessageTestContext(() => Task.FromException(new PermanentException("Invalid message")));

        await context.ProcessAsync();

        CollectionAssert.AreEqual(new[] { "dead-letter" }, context.Receiver.Settlements);
        Assert.IsTrue(context.Processors[0].Disposed);
    }

    [TestMethod]
    [DataRow(true, "dead-letter")]
    [DataRow(false, "abandon")]
    public async Task ProcessMessageAsync_CodedFailureUsesPermanenceAsync(bool isPermanent, string settlement)
    {
        using var context = new MessageTestContext(() => Task.FromException(new TestCodedException(isPermanent)));

        await context.ProcessAsync();

        CollectionAssert.AreEqual(new[] { settlement }, context.Receiver.Settlements);
        Assert.IsTrue(context.Processors[0].Disposed);
    }

    [TestMethod]
    public async Task ProcessMessageAsync_TransientFailureAbandonsAsync()
    {
        using var context = new MessageTestContext(() => Task.FromException(new InvalidOperationException("Try again")));

        await context.ProcessAsync();

        CollectionAssert.AreEqual(new[] { "abandon" }, context.Receiver.Settlements);
        Assert.IsTrue(context.Processors[0].Disposed);
    }

    [TestMethod]
    public async Task ProcessMessageAsync_LockLostDoesNotAttemptSettlementAsync()
    {
        var failure = new ServiceBusException("Lock expired", ServiceBusFailureReason.MessageLockLost);
        using var context = new MessageTestContext(() => Task.FromException(failure));

        var actual = await Assert.ThrowsExactlyAsync<ServiceBusException>(context.ProcessAsync);

        Assert.AreSame(failure, actual);
        Assert.HasCount(0, context.Receiver.Settlements);
        Assert.IsTrue(context.Processors[0].Disposed);
    }

    [TestMethod]
    [DataRow("complete")]
    [DataRow("abandon")]
    [DataRow("dead-letter")]
    public async Task ProcessMessageAsync_SettlementFailureDoesNotAttemptSecondSettlementAsync(string settlement)
    {
        var handlerFailure = settlement switch
        {
            "abandon" => new InvalidOperationException("Retry"),
            "dead-letter" => (Exception)new PermanentException("Invalid"),
            _ => null,
        };
        using var context = new MessageTestContext(() => handlerFailure == null ? Task.CompletedTask : Task.FromException(handlerFailure));
        var failure = new ServiceBusException("Lock expired", ServiceBusFailureReason.MessageLockLost);
        context.Receiver.SettlementFailure = failure;

        var actual = await Assert.ThrowsExactlyAsync<ServiceBusException>(context.ProcessAsync);

        Assert.AreSame(failure, actual);
        CollectionAssert.AreEqual(new[] { settlement }, context.Receiver.Settlements);
        Assert.IsTrue(context.Processors[0].Disposed);
    }

    #endregion

    #region Test Helpers

    private sealed class TestServiceBusProcessor : ServiceBusProcessor
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Stopping { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Drained { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public CancellationToken StopToken { get; private set; }
        public Exception StartFailure { get; init; }

        public override Task StartProcessingAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Started.TrySetResult();
            return StartFailure == null ? Task.CompletedTask : Task.FromException(StartFailure);
        }

        public override Task StopProcessingAsync(CancellationToken cancellationToken = default)
        {
            StopToken = cancellationToken;
            Stopping.TrySetResult();
            return Drained.Task;
        }
    }

    private sealed class TestReceiver : ServiceBusReceiver
    {
        public List<string> Settlements { get; } = [];
        public Exception SettlementFailure { get; set; }

        private Task SettleAsync(string settlement)
        {
            Settlements.Add(settlement);
            return SettlementFailure == null ? Task.CompletedTask : Task.FromException(SettlementFailure);
        }

        public override Task CompleteMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
            => SettleAsync("complete");

        public override Task AbandonMessageAsync(ServiceBusReceivedMessage message, IDictionary<string, object> propertiesToModify = default, CancellationToken cancellationToken = default)
            => SettleAsync("abandon");

        public override Task DeadLetterMessageAsync(ServiceBusReceivedMessage message, IDictionary<string, object> propertiesToModify = default, CancellationToken cancellationToken = default)
            => SettleAsync("dead-letter");
    }

    private sealed class MessageTestContext : IDisposable, IInboundMessageExecutor, IConnectionStringProvider
    {
        private readonly ServiceProvider Services;
        private readonly ServiceBusBackgroundService Service;
        public TestReceiver Receiver { get; } = new();
        public List<TestMessageProcessor> Processors { get; } = [];
        public string ResolvedName { get; private set; }

        public MessageTestContext(Func<Task> processAsync)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IInboundMessageExecutor>(this);
            services.AddScoped<IInboundMessageProcessor>(_ =>
            {
                var processor = new TestMessageProcessor(processAsync);
                Processors.Add(processor);
                return processor;
            });
            services.AddScoped<INamedFactory>(sp => new TestNamedFactory(sp, name => ResolvedName = name));
            Services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            Service = new(null, this, Options.Create(new ServiceBusBackgroundService.Config()), new(Services, NullLoggerFactory.Instance));
        }

        public Task ProcessAsync()
        {
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("message"), messageId: "test", sequenceNumber: 1);
            return Service.ProcessMessageAsync(new() { QueueName = "queue", MessageWorkerTypeName = "worker" }, new(message, Receiver, CancellationToken.None));
        }

        Task IInboundMessageExecutor.ExecuteAsync(IInboundMessage message, Func<IInboundMessage, Task> executeAsync)
            => executeAsync(message);

        string IConnectionStringProvider.GetConnectionString(string connectionStringName)
            => throw new NotSupportedException();

        public void Dispose()
        {
            Service.Dispose();
            Services.Dispose();
        }
    }

    private sealed class TestNamedFactory(IServiceProvider serviceProvider, Action<string> resolved) : INamedFactory
    {
        T INamedFactory.GetServiceByName<T>(string name)
        {
            resolved(name);
            return serviceProvider.GetRequiredService<T>();
        }
    }

    private sealed class TestMessageProcessor(Func<Task> processAsync) : IInboundMessageProcessor, IDisposable
    {
        public bool Disposed { get; private set; }

        Task IInboundMessageProcessor.ProcessInboundMessageAsync(IInboundMessage msg)
            => processAsync();

        public void Dispose()
            => Disposed = true;
    }

    private sealed class TestCodedException : BaseCodedException
    {
        public TestCodedException(bool isPermanent)
            => IsPermanent = isPermanent;

        public override object GetCode()
            => "test";
    }

    #endregion
}
