using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus;

public class DefaultServiceBusMessageSender : ServiceBusMessageSender
{
    public DefaultServiceBusMessageSender(ServiceBusMessageSenderConstructorArgs constructorArgs, ILogger<DefaultServiceBusMessageSender> logger)
        : base(constructorArgs, logger)
    { }
}
