using System.IO;
using Azure.Messaging.ServiceBus;

namespace RevolutionaryStuff.Azure.Services.Messaging.Inbound;

public class InboundMessage : IInboundMessage
{
    private readonly IInboundMessage I;
    private readonly BinaryData BinaryData;

    public string MessageId { get; }
    public string ContentType { get; }
    public long SequenceNumber { get; }
    public string CorrelationId { get; }
    public string Subject { get; }
    public bool MessageRetrievedFromStorage { get; }

    internal readonly IDictionary<string, object> PropertiesField;
    public IDictionary<string, object> DeliveryProperties { get; init; }

    IDictionary<string, object> IInboundMessage.Properties
        => PropertiesField ?? Empty.StringObjectDictionary;

    private Stream BodyAsStreamField;

    Stream IInboundMessage.BodyAsStream
        => BodyAsStreamField ??= BinaryData.ToStream();

    private string BodyAsStringField;
    private readonly string SubjectField;

    string IInboundMessage.BodyAsString
        => BodyAsStringField ??= BinaryData.ToString();

    public DateTimeOffset EnqueuedTime { get; }

    TVal IInboundMessage.GetPropertyVal<TVal>(string key, TVal missing)
    {
        if (I.Properties.ContainsKey(key))
        {
            try
            {
                return (TVal)I.Properties[key];
            }
            catch (Exception) { }
        }
        return missing;
    }

    TVal IInboundMessage.GetConvertedPropertyVal<TVal>(string key, TVal missing, bool throwOnConversionIssue)
    {
        if (I.Properties.ContainsKey(key))
        {
            var o = I.Properties[key];
            try
            {
                if (o is TVal val) return val;
                var oc = Convert.ChangeType(o, typeof(TVal));
                return (TVal)oc;
            }
            catch (Exception) { }
            if (throwOnConversionIssue)
            {
                return (TVal)o;
            }
        }
        return missing;
    }

    private static IEnumerable<KeyValuePair<string, object>> PropertiesWithTenantId(IEnumerable<KeyValuePair<string, object>> properties, string tenantId)
    {
        var ret = new List<KeyValuePair<string, object>>();
        if (properties != null)
        {
            ret.AddRange(properties);
        }
        if (!ret.Any(kvp => kvp.Key == MessageHelpers.PropertyNames.TenantId))
        {
            ret.Add(new KeyValuePair<string, object>(MessageHelpers.PropertyNames.TenantId, tenantId));
        }
        return ret;
    }

    public static IInboundMessage Create(
        byte[] messageBody,
        string contentType = null,
        string messageId = null,
        IEnumerable<KeyValuePair<string, object>> properties = null,
        long sequenceNumber = 0,
        string tenantId = null,
        string subject = null,
        bool messageRetrievedFromStorage = false)
        => new InboundMessage(
            new BinaryData(messageBody),
            contentType,
            messageId,
            PropertiesWithTenantId(properties, tenantId),
            sequenceNumber,
            null,
            DateTimeOffset.UtcNow,
            subject,
            messageRetrievedFromStorage);

    public static IInboundMessage Create(
        string messageBody,
        string contentType = null,
        string messageId = null,
        IEnumerable<KeyValuePair<string, object>> properties = null,
        long sequenceNumber = 0,
        string tenantId = null,
        string subject = null,
        bool messageRetrievedFromStorage = false)
        => new InboundMessage(
            new BinaryData(messageBody),
            contentType,
            messageId,
            PropertiesWithTenantId(properties, tenantId),
            sequenceNumber,
            null,
            DateTimeOffset.UtcNow,
            subject,
            messageRetrievedFromStorage);

    public static IInboundMessage Create(ServiceBusReceivedMessage sbrm)
        => new InboundMessage(
            sbrm.Body,
            sbrm.ContentType,
            sbrm.MessageId,
            sbrm.ApplicationProperties,
            sbrm.SequenceNumber,
            sbrm.CorrelationId,
            sbrm.EnqueuedTime,
            sbrm.Subject)
        {
            DeliveryProperties = sbrm.GetRawAmqpMessage().DeliveryAnnotations
        };

    private InboundMessage(
        BinaryData binaryData,
        string contentType,
        string messageId,
        IEnumerable<KeyValuePair<string, object>> properties,
        long sequenceNumber,
        string correlationId,
        DateTimeOffset enqueuedTime,
        string subject,
        bool messageRetrievedFromStorage = false)
    {
        I = this;
        SequenceNumber = sequenceNumber;
        BinaryData = binaryData;
        ContentType = contentType;
        MessageId = messageId;
        if (properties.NullSafeAny())
        {
            PropertiesField = new Dictionary<string, object>(properties);
        }
        CorrelationId = correlationId ?? string.Empty;
        EnqueuedTime = enqueuedTime;
        SubjectField = subject;
        MessageRetrievedFromStorage = messageRetrievedFromStorage;
    }
}
