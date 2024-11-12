using System.IO;
using System.Text.Json;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;

namespace RevolutionaryStuff.Data.Cosmos.Workers;
internal class CosmosReceivedMessage(string Id, JsonElement El, string DatabaseName, string ContainerName, long SequenceNumber, DateTimeOffset TouchedAt, IDictionary<string, object> Properties) : ICosmosReceivedMessage
{
    string ICosmosReceivedMessage.DatabaseName => DatabaseName;

    string ICosmosReceivedMessage.ContainerName => ContainerName;

    JsonElement ICosmosReceivedMessage.DocumentElement => El;

    string IInboundMessage.MessageId => Id;

    string IInboundMessage.ContentType
        => MimeType.Application.Json.PrimaryContentType;

    long IInboundMessage.SequenceNumber
        => SequenceNumber;

    string IInboundMessage.CorrelationId
        => null;

    DateTimeOffset IInboundMessage.EnqueuedTime
        => TouchedAt;

    IDictionary<string, object> IInboundMessage.Properties
        => Properties ?? Empty.StringObjectDictionary;

    IDictionary<string, object> IInboundMessage.DeliveryProperties
        => Empty.StringObjectDictionary;

    private string BodyAsString;
    string IInboundMessage.BodyAsString
        => BodyAsString ??= El.ToString();

    Stream IInboundMessage.BodyAsStream
        => StreamHelpers.Create(BodyAsString);

    string IInboundMessage.Subject
        => $"{DatabaseName}.{ContainerName}";

    TVal IInboundMessage.GetConvertedPropertyVal<TVal>(string key, TVal missing, bool throwOnConversionIssue)
        => missing;
    TVal IInboundMessage.GetPropertyVal<TVal>(string key, TVal missing)
        => missing;
}

