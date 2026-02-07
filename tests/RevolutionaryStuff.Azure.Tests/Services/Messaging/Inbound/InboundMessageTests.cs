using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;

namespace RevolutionaryStuff.Azure.Tests.Services.Messaging.Inbound;

[TestClass]
public class InboundMessageTests
{
    private const string TestMessageBody = "Test message body";
    private const string TestContentType = "application/json";
    private const string TestMessageId = "test-message-id-123";
    private const string TestSubject = "test-subject";
    private const string TestTenantId = "tenant-123";

    #region BinaryData Constructor Tests - String

    [TestMethod]
    public void Create_WithStringMessageBody_CreatesValidMessage()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody,
            contentType: TestContentType,
            messageId: TestMessageId);

        Assert.IsNotNull(message);
        Assert.AreEqual(TestContentType, message.ContentType);
        Assert.AreEqual(TestMessageId, message.MessageId);
    }

    [TestMethod]
    public void Create_WithStringMessageBody_BodyAsStringReturnsOriginal()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody,
            contentType: TestContentType);

        Assert.AreEqual(TestMessageBody, message.BodyAsString);
    }

    [TestMethod]
    public void Create_WithEmptyString_CreatesValidMessage()
    {
        var message = InboundMessage.Create(
            messageBody: string.Empty,
            contentType: TestContentType);

        Assert.IsNotNull(message);
        Assert.AreEqual(string.Empty, message.BodyAsString);
    }

    [TestMethod]
    public void Create_WithStringMessageBody_AndNullContentType_CreatesValidMessage()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody,
            contentType: null);

        Assert.IsNotNull(message);
        Assert.IsNull(message.ContentType);
    }

    #endregion

    #region BinaryData Constructor Tests - Byte Array

    [TestMethod]
    public void Create_WithByteArrayMessageBody_CreatesValidMessage()
    {
        var bodyBytes = Encoding.UTF8.GetBytes(TestMessageBody);

        var message = InboundMessage.Create(
            messageBody: bodyBytes,
            contentType: TestContentType,
            messageId: TestMessageId);

        Assert.IsNotNull(message);
        Assert.AreEqual(TestContentType, message.ContentType);
        Assert.AreEqual(TestMessageId, message.MessageId);
    }

    [TestMethod]
    public void Create_WithByteArrayMessageBody_BodyAsStringReturnsOriginal()
    {
        var bodyBytes = Encoding.UTF8.GetBytes(TestMessageBody);

        var message = InboundMessage.Create(
            messageBody: bodyBytes,
            contentType: TestContentType);

        Assert.AreEqual(TestMessageBody, message.BodyAsString);
    }

    [TestMethod]
    public void Create_WithEmptyByteArray_CreatesValidMessage()
    {
        var message = InboundMessage.Create(
            messageBody: Array.Empty<byte>(),
            contentType: TestContentType);

        Assert.IsNotNull(message);
        Assert.AreEqual(string.Empty, message.BodyAsString);
    }

    [TestMethod]
    public void Create_WithByteArrayMessageBody_AndNullContentType_CreatesValidMessage()
    {
        var bodyBytes = Encoding.UTF8.GetBytes(TestMessageBody);

        var message = InboundMessage.Create(
            messageBody: bodyBytes,
            contentType: null);

        Assert.IsNotNull(message);
        Assert.IsNull(message.ContentType);
    }

    #endregion

    #region ToString() Method Tests (BinaryData.ToString)

    [TestMethod]
    public void BodyAsString_ReturnsCorrectString()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody);

        var result = message.BodyAsString;

        Assert.AreEqual(TestMessageBody, result);
    }

    [TestMethod]
    public void BodyAsString_CalledMultipleTimes_ReturnsSameValue()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody);

        var result1 = message.BodyAsString;
        var result2 = message.BodyAsString;

        Assert.AreSame(result1, result2);
    }

    [TestMethod]
    public void BodyAsString_WithSpecialCharacters_ReturnsCorrectString()
    {
        const string specialChars = "Special chars: äöü ñ 中文";
        var message = InboundMessage.Create(
            messageBody: specialChars);

        Assert.AreEqual(specialChars, message.BodyAsString);
    }

    [TestMethod]
    public void BodyAsString_WithJsonContent_ReturnsCorrectString()
    {
        const string jsonContent = "{\"name\":\"test\",\"value\":123}";
        var message = InboundMessage.Create(
            messageBody: jsonContent,
            contentType: "application/json");

        Assert.AreEqual(jsonContent, message.BodyAsString);
    }

    #endregion

    #region ToStream() Method Tests (BinaryData.ToStream)

    [TestMethod]
    public void BodyAsStream_ReturnsReadableStream()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody);

        var stream = message.BodyAsStream;

        Assert.IsNotNull(stream);
        Assert.IsTrue(stream.CanRead);
    }

    [TestMethod]
    public void BodyAsStream_ContainsCorrectData()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody);

        var stream = message.BodyAsStream;
        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();

        Assert.AreEqual(TestMessageBody, result);
    }

    [TestMethod]
    public void BodyAsStream_CalledMultipleTimes_ReturnsSameStream()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody);

        var stream1 = message.BodyAsStream;
        var stream2 = message.BodyAsStream;

        Assert.AreSame(stream1, stream2);
    }

    [TestMethod]
    public void BodyAsStream_FromByteArray_ContainsCorrectData()
    {
        var bodyBytes = Encoding.UTF8.GetBytes(TestMessageBody);
        var message = InboundMessage.Create(
            messageBody: bodyBytes);

        var stream = message.BodyAsStream;
        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();

        Assert.AreEqual(TestMessageBody, result);
    }

    [TestMethod]
    public void BodyAsStream_WithEmptyContent_ReturnsEmptyStream()
    {
        var message = InboundMessage.Create(
            messageBody: string.Empty);

        var stream = message.BodyAsStream;
        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();

        Assert.AreEqual(string.Empty, result);
    }

    #endregion

    #region Message Properties Tests

    [TestMethod]
    public void Create_WithProperties_PropertiesAreAccessible()
    {
        var properties = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 }
        };

        var message = InboundMessage.Create(
            messageBody: TestMessageBody,
            properties: properties);

        Assert.AreEqual("value1", message.GetPropertyVal<string>("key1"));
        Assert.AreEqual(42, message.GetPropertyVal<int>("key2"));
    }

    [TestMethod]
    public void Create_WithTenantId_TenantIdIsInProperties()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody,
            tenantId: TestTenantId);

        Assert.AreEqual(TestTenantId, message.GetPropertyVal<string>("tenantId"));
    }

    [TestMethod]
    public void Create_WithSequenceNumber_SequenceNumberIsSet()
    {
        const long sequenceNumber = 12345L;

        var message = InboundMessage.Create(
            messageBody: TestMessageBody,
            sequenceNumber: sequenceNumber);

        Assert.AreEqual(sequenceNumber, message.SequenceNumber);
    }

    [TestMethod]
    public void Create_WithMessageRetrievedFromStorage_FlagIsSet()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody,
            messageRetrievedFromStorage: true);

        // MessageRetrievedFromStorage is only on the concrete InboundMessage class, not the interface
        var concreteMessage = message as InboundMessage;
        Assert.IsNotNull(concreteMessage);
        Assert.IsTrue(concreteMessage.MessageRetrievedFromStorage);
    }

    [TestMethod]
    public void GetPropertyVal_NonExistentKey_ReturnsMissingValue()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody);

        var result = message.GetPropertyVal("nonexistent", "default");

        Assert.AreEqual("default", result);
    }

    [TestMethod]
    public void GetConvertedPropertyVal_ConvertsIntToString()
    {
        var properties = new Dictionary<string, object>
        {
            { "intValue", 42 }
        };

        var message = InboundMessage.Create(
            messageBody: TestMessageBody,
            properties: properties);

        var result = message.GetConvertedPropertyVal<string>("intValue", "default");

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void GetConvertedPropertyVal_NonExistentKey_ReturnsMissingValue()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody);

        var result = message.GetConvertedPropertyVal<int>("nonexistent", -1);

        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void Create_WithNullProperties_DoesNotThrow()
    {
        var message = InboundMessage.Create(
            messageBody: TestMessageBody,
            properties: null);

        Assert.IsNotNull(message);
        Assert.IsNotNull(message.Properties);
    }

    #endregion

    #region ServiceBusReceivedMessage Creation Tests

    [TestMethod]
    public void Create_FromServiceBusReceivedMessage_ExtractsBody()
    {
        var sbrm = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(TestMessageBody),
            messageId: TestMessageId,
            contentType: TestContentType);

        var message = InboundMessage.Create(sbrm);

        Assert.IsNotNull(message);
        Assert.AreEqual(TestMessageBody, message.BodyAsString);
    }

    [TestMethod]
    public void Create_FromServiceBusReceivedMessage_ExtractsMessageId()
    {
        var sbrm = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(TestMessageBody),
            messageId: TestMessageId);

        var message = InboundMessage.Create(sbrm);

        Assert.AreEqual(TestMessageId, message.MessageId);
    }

    [TestMethod]
    public void Create_FromServiceBusReceivedMessage_ExtractsContentType()
    {
        var sbrm = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(TestMessageBody),
            contentType: TestContentType);

        var message = InboundMessage.Create(sbrm);

        Assert.AreEqual(TestContentType, message.ContentType);
    }

    [TestMethod]
    public void Create_FromServiceBusReceivedMessage_ExtractsSequenceNumber()
    {
        const long sequenceNumber = 98765L;
        var sbrm = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(TestMessageBody),
            sequenceNumber: sequenceNumber);

        var message = InboundMessage.Create(sbrm);

        Assert.AreEqual(sequenceNumber, message.SequenceNumber);
    }

    [TestMethod]
    public void Create_FromServiceBusReceivedMessage_ExtractsCorrelationId()
    {
        const string correlationId = "correlation-123";
        var sbrm = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(TestMessageBody),
            correlationId: correlationId);

        var message = InboundMessage.Create(sbrm);

        Assert.AreEqual(correlationId, message.CorrelationId);
    }

    [TestMethod]
    public void Create_FromServiceBusReceivedMessage_ExtractsEnqueuedTime()
    {
        var enqueuedTime = DateTimeOffset.UtcNow.AddMinutes(-5);
        var sbrm = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(TestMessageBody),
            enqueuedTime: enqueuedTime);

        var message = InboundMessage.Create(sbrm);

        Assert.AreEqual(enqueuedTime, message.EnqueuedTime);
    }

    [TestMethod]
    public void Create_FromServiceBusReceivedMessage_ExtractsApplicationProperties()
    {
        var properties = new Dictionary<string, object>
        {
            { "customProp", "customValue" }
        };
        var sbrm = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(TestMessageBody),
            properties: properties);

        var message = InboundMessage.Create(sbrm);

        Assert.AreEqual("customValue", message.GetPropertyVal<string>("customProp"));
    }

    [TestMethod]
    public void Create_FromServiceBusReceivedMessage_WithMultipleProperties_ExtractsAll()
    {
        var properties = new Dictionary<string, object>
        {
            { "stringProp", "stringValue" },
            { "intProp", 42 },
            { "boolProp", true }
        };
        var sbrm = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(TestMessageBody),
            properties: properties);

        var message = InboundMessage.Create(sbrm);

        Assert.AreEqual("stringValue", message.GetPropertyVal<string>("stringProp"));
        Assert.AreEqual(42, message.GetPropertyVal<int>("intProp"));
        Assert.AreEqual(true, message.GetPropertyVal<bool>("boolProp"));
    }

    [TestMethod]
    public void Create_FromServiceBusReceivedMessage_BodyAsStreamWorks()
    {
        var sbrm = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(TestMessageBody));

        var message = InboundMessage.Create(sbrm);

        var stream = message.BodyAsStream;
        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();

        Assert.AreEqual(TestMessageBody, result);
    }

    #endregion
}
