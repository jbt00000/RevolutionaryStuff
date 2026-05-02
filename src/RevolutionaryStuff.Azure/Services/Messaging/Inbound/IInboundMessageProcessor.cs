namespace RevolutionaryStuff.Azure.Services.Messaging.Inbound;

/// <summary>
/// Processes a single inbound message received from a message transport (e.g., Service Bus, Cosmos Change Feed).
/// Implementations contain the application-level logic for handling a given message type.
/// </summary>
public interface IInboundMessageProcessor
{
    /// <summary>
    /// Handles the given <paramref name="msg"/>, performing all application-level work associated with it.
    /// </summary>
    /// <param name="msg">The inbound message to process.</param>
    Task ProcessInboundMessageAsync(IInboundMessage msg);
}
