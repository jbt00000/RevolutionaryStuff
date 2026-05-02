using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Data.Cosmos.BackgroundServices;
using RevolutionaryStuff.Data.JsonStore.ChangeDataCapture;
using RevolutionaryStuff.Data.JsonStore.Cosmos.ChangeDataCapture;
using RevolutionaryStuff.Data.JsonStore.Entities;
using static RevolutionaryStuff.Core.RevolutionaryStuffService;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.ChangeDataCapture;

[TestClass]
public class CosmosChangeFeedEntityActorProcessorTests
{
    #region Test doubles

    [JsonEntityAbbreviation("testEntity")]
    private sealed class TestJsonEntity : JsonEntity
    {
        public string Value { get; set; }
    }

    private sealed class TestActor : IChangeDataCaptureJsonEntityController
    {
        public List<TestJsonEntity> Received { get; } = new();

        [ChangeDataCaptureJsonEntityActorAttribute]
        public Task HandleAsync(TestJsonEntity entity)
        {
            Received.Add(entity);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCosmosMessage : ICosmosInboundMessage
    {
        private readonly JsonElement DocumentEl;
        private readonly string DataTypeVal;
        private readonly string DocumentIdVal;

        public FakeCosmosMessage(JsonElement documentEl, string dataType, string documentId)
        {
            DocumentEl = documentEl;
            DataTypeVal = dataType;
            DocumentIdVal = documentId;
        }

        string ICosmosInboundMessage.DatabaseName => "testDb";
        string ICosmosInboundMessage.ContainerName => "testContainer";
        JsonElement ICosmosInboundMessage.DocumentElement => DocumentEl;

        string IInboundMessage.MessageId => "test-id";
        string IInboundMessage.ContentType => MimeType.Application.Json.PrimaryContentType;
        long IInboundMessage.SequenceNumber => 1;
        string IInboundMessage.CorrelationId => null;
        DateTimeOffset IInboundMessage.EnqueuedTime => DateTimeOffset.UtcNow;
        IDictionary<string, object> IInboundMessage.Properties => new Dictionary<string, object>();
        IDictionary<string, object> IInboundMessage.DeliveryProperties => new Dictionary<string, object>();
        string IInboundMessage.BodyAsString => DocumentEl.ToString();
        Stream IInboundMessage.BodyAsStream => StreamHelpers.Create(DocumentEl.ToString());
        string IInboundMessage.Subject => "testDb.testContainer";
        TVal IInboundMessage.GetPropertyVal<TVal>(string key, TVal missing) => missing;
        TVal IInboundMessage.GetConvertedPropertyVal<TVal>(string key, TVal missing, bool throwOnConversionIssue) => missing;
    }

    private static CosmosChangeDataCaptureJsonEntityEventProcessor CreateProcessor(IServiceProvider serviceProvider)
    {
        var loggerFactory = NullLoggerFactory.Instance;
        var args = new CosmosChangeDataCaptureJsonEntityEventProcessor.CosmosChangeDataCaptureJsonEntityEventProcessorConstructorArgs(
            serviceProvider,
            new RevolutionaryStuffServiceConstrutorArge(loggerFactory));
        return new CosmosChangeDataCaptureJsonEntityEventProcessor(args);
    }

    private static JsonElement BuildDocumentElement(string dataType, string id, string value = "test")
    {
        var json = $@"{{""_jet"":""{dataType}"",""id"":""{id}"",""Value"":""{value}""}}";
        return JsonDocument.Parse(json).RootElement;
    }

    #endregion

    [TestMethod]
    public async Task ProcessInboundMessageAsync_WhenMessageIsNotCosmosEvent_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var processor = CreateProcessor(services.BuildServiceProvider());

        var nonCosmosMsg = InboundMessage.Create("body", "application/json", "id-1");
        await ((IInboundMessageProcessor)processor).ProcessInboundMessageAsync(nonCosmosMsg);
    }

    [TestMethod]
    public async Task ProcessInboundMessageAsync_DispatchesToMatchingActor()
    {
        var actor = new TestActor();
        var services = new ServiceCollection();
        services.AddSingleton<IChangeDataCaptureJsonEntityController>(actor);
        services.AddSingleton(actor);
        var processor = CreateProcessor(services.BuildServiceProvider());

        var entity = new TestJsonEntity { Value = "hello" };
        var dataType = JsonEntity.GetDataType<TestJsonEntity>();
        var docEl = BuildDocumentElement(dataType, entity.Id, "hello");
        var msg = new FakeCosmosMessage(docEl, dataType, entity.Id);

        await ((IInboundMessageProcessor)processor).ProcessInboundMessageAsync(msg);

        Assert.AreEqual(1, actor.Received.Count);
        Assert.AreEqual("hello", actor.Received[0].Value);
    }

    [TestMethod]
    public async Task ProcessInboundMessageAsync_WithNoMatchingActors_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var processor = CreateProcessor(services.BuildServiceProvider());

        var dataType = JsonEntity.GetDataType<TestJsonEntity>();
        var docEl = BuildDocumentElement(dataType, "some-id");
        var msg = new FakeCosmosMessage(docEl, dataType, "some-id");

        await ((IInboundMessageProcessor)processor).ProcessInboundMessageAsync(msg);
    }

    [TestMethod]
    public async Task ProcessInboundMessageAsync_DispatchesToActorOnce_WhenCalledTwice()
    {
        var actor = new TestActor();
        var services = new ServiceCollection();
        services.AddSingleton<IChangeDataCaptureJsonEntityController>(actor);
        services.AddSingleton(actor);
        var processor = CreateProcessor(services.BuildServiceProvider());

        var dataType = JsonEntity.GetDataType<TestJsonEntity>();
        var docEl = BuildDocumentElement(dataType, "doc-1");
        var msg = new FakeCosmosMessage(docEl, dataType, "doc-1");

        await ((IInboundMessageProcessor)processor).ProcessInboundMessageAsync(msg);
        await ((IInboundMessageProcessor)processor).ProcessInboundMessageAsync(msg);

        Assert.AreEqual(2, actor.Received.Count);
    }

    [TestMethod]
    public async Task ProcessInboundMessageAsync_WithUnknownDataType_DoesNotDispatch()
    {
        var actor = new TestActor();
        var services = new ServiceCollection();
        services.AddSingleton<IChangeDataCaptureJsonEntityController>(actor);
        services.AddSingleton(actor);
        var processor = CreateProcessor(services.BuildServiceProvider());

        var docEl = BuildDocumentElement("unknown.type", "doc-1");
        var msg = new FakeCosmosMessage(docEl, "unknown.type", "doc-1");

        await ((IInboundMessageProcessor)processor).ProcessInboundMessageAsync(msg);

        Assert.AreEqual(0, actor.Received.Count);
    }
}
