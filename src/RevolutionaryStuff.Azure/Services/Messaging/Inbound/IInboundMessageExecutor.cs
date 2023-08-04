using System.Runtime.CompilerServices;
using Azure.Messaging.ServiceBus;

namespace RevolutionaryStuff.Azure.Services.Messaging.Inbound;

public interface IInboundMessageExecutor
{
    Task ExecuteAsync(IInboundMessage message, Func<IInboundMessage, Task> executeAsync, [CallerMemberName] string caller = null);

    #region Default Implementations

    Task ExecuteAsync(ServiceBusReceivedMessage message, Func<IInboundMessage, Task> executeAsync, [CallerMemberName] string caller = null)
        => ExecuteAsync(InboundMessage.Create(message), executeAsync, caller);

    #endregion
}
