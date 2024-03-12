using System.Text.Json;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;

namespace RevolutionaryStuff.Data.Cosmos.Workers;

public interface ICosmosReceivedMessage : IInboundMessage
{
    string DatabaseName { get; }
    string ContainerName { get; }
    JsonElement DocumentElement { get; }
}

