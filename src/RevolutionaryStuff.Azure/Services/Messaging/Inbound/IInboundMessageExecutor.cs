using Azure.Messaging.ServiceBus;

namespace RevolutionaryStuff.Azure.Services.Messaging.Inbound;

/// <summary>
/// Coordinates the execution of an inbound message, providing a pipeline hook
/// (e.g., for error handling, retries, or telemetry) that wraps the core processing delegate.
/// </summary>
public interface IInboundMessageExecutor
{
    /// <summary>
    /// Executes the supplied <paramref name="executeAsync"/> delegate for the given <paramref name="message"/>,
    /// applying any cross-cutting concerns implemented by this executor (e.g., dead-lettering, retry, tracing).
    /// </summary>
    /// <param name="message">The inbound message to process.</param>
    /// <param name="executeAsync">The delegate that performs the actual message processing.</param>
    Task ExecuteAsync(IInboundMessage message, Func<IInboundMessage, Task> executeAsync);

    #region Default Implementations

    /// <summary>
    /// Convenience overload that adapts a raw <see cref="ServiceBusReceivedMessage"/> into an
    /// <see cref="IInboundMessage"/> before forwarding to <see cref="ExecuteAsync(IInboundMessage, Func{IInboundMessage, Task})"/>.
    /// </summary>
    /// <param name="message">The Service Bus message to adapt and process.</param>
    /// <param name="executeAsync">The delegate that performs the actual message processing.</param>
    Task ExecuteAsync(ServiceBusReceivedMessage message, Func<IInboundMessage, Task> executeAsync)
        => ExecuteAsync(InboundMessage.Create(message), executeAsync);

    #endregion
}
