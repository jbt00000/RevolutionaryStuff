﻿using System.IO;

namespace RevolutionaryStuff.Azure.Services.Messaging.Inbound;
public interface IInboundMessage
{
    string MessageId { get; }
    string ContentType { get; }
    long SequenceNumber { get; }
    string CorrelationId { get; }
    DateTimeOffset EnqueuedTime { get; }
    IDictionary<string, object> Properties { get; }
    IDictionary<string, object> DeliveryProperties { get; }
    TVal GetPropertyVal<TVal>(string key, TVal missing = default);
    TVal GetConvertedPropertyVal<TVal>(string key, TVal missing = default, bool throwOnConversionIssue = false);
    string BodyAsString { get; }
    Stream BodyAsStream { get; }
    string Subject { get; }
}
