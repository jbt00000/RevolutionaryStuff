using System.Text.Json;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;

namespace RevolutionaryStuff.Data.Cosmos.BackgroundServices;

public interface ICosmosInboundMessage : IInboundMessage
{
    string DatabaseName { get; }
    string ContainerName { get; }
    JsonElement DocumentElement { get; }
}

