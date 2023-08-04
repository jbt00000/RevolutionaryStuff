using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.Services.Correlation;

namespace RevolutionaryStuff.Azure.Services.Messaging.Inbound;

public class InboundMessageExecutor : BaseLoggingDisposable, IInboundMessageExecutor
{
    private readonly HardcodedCorrelationIdFinder CorrelationIdFinder;

    public InboundMessageExecutor(
        HardcodedCorrelationIdFinder correlationIdFinder,
        ILogger<InboundMessageExecutor> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(correlationIdFinder);

        CorrelationIdFinder = correlationIdFinder;
    }

    async Task IInboundMessageExecutor.ExecuteAsync(IInboundMessage message, Func<IInboundMessage, Task> executeAsync, string caller)
    {
        if (message.Properties.ContainsKey(MessageHelpers.PropertyNames.TenantId))
        {
            RegisterDisposableObject(LogScopedProperty(MessageHelpers.PropertyNames.TenantId, message.Properties[MessageHelpers.PropertyNames.TenantId]));
        }

        RegisterDisposableObject(LogScopedProperty("message.SequenceNumber", message.SequenceNumber));
        RegisterDisposableObject(LogScopedProperty("message.EnqueuedTime", message.EnqueuedTime));

        CorrelationIdFinder.CorrelationId = message.CorrelationId;

        await executeAsync(message);
    }
}
