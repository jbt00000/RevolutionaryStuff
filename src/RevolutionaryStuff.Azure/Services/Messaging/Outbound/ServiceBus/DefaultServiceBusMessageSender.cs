using static RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus.ServiceBusMessageSender;

namespace RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus;

public class DefaultServiceBusMessageSender(ServiceBusMessageSenderConstructorArgs baseConstructorArgs)
    : ServiceBusMessageSender(baseConstructorArgs)
{ }
