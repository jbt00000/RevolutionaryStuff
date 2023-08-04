namespace RevolutionaryStuff.Azure.Services.Messaging.Inbound;

public interface IInboundMessageProcessor
{
    Task ProcessInboundMessageAsync(IInboundMessage msg);
}
