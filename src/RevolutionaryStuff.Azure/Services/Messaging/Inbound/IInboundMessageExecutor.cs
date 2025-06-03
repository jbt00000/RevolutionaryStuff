using Azure.Messaging.ServiceBus;

namespace RevolutionaryStuff.Azure.Services.Messaging.Inbound;

public interface IInboundMessageExecutor
{
    Task ExecuteAsync(IInboundMessage message, Func<IInboundMessage, Task> executeAsync);

    #region Default Implementations

    Task ExecuteAsync(ServiceBusReceivedMessage message, Func<IInboundMessage, Task> executeAsync)
        => ExecuteAsync(InboundMessage.Create(message), executeAsync);

    #endregion
}
