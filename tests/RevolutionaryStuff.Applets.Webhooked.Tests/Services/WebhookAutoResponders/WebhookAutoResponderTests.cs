using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RevolutionaryStuff.Applets.Blobs;
using RevolutionaryStuff.Applets.Webhooked;
using RevolutionaryStuff.ApiCore.Services;
using RevolutionaryStuff.AspNetCore.Services.SazGenerators;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Services.TemporaryStreamFactory;

namespace RevolutionaryStuff.Applets.Webhooked.Tests;

[TestClass]
public class WebhookAutoResponderTests
{
    private class TestBlobWriter : IWebhookedDiagnosticBlobWriter
    {
        public List<(string name, long size, WriteBlobSettings? settings)> WrittenBlobs { get; } = new();

        public Task<WriteBlobResult> WriteBlobAsync(string name, Stream st, WriteBlobSettings? settings = null)
        {
            var size = st.Length;
            WrittenBlobs.Add((name, size, settings));
            return Task.FromResult(new WriteBlobResult { StorageName = "test", Name = name, Size = size });
        }
    }

    private WebhookAutoResponder<TestBlobWriter> CreateResponder(
        WebhookAutoResponderConfig config,
        out TestBlobWriter blobWriter,
        out Mock<IServiceBusMessageSender> mockServiceBusSender,
        HttpContext? httpContext = null)
    {
        blobWriter = new TestBlobWriter();
        var mockWebSessionArchiver = new Mock<IWebSessionArchiver>();
        mockWebSessionArchiver.Setup(x => x.FileExtension).Returns(".saz");
        mockWebSessionArchiver.Setup(x => x.GenerateAsync(It.IsAny<IList<WebSession>>(), It.IsAny<Stream>()))
            .Returns<IList<WebSession>, Stream>((sessions, stream) =>
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes("mock-saz-content");
                stream.Write(bytes, 0, bytes.Length);
                return Task.CompletedTask;
            });

        var mockStreamFactory = new Mock<ITemporaryStreamFactory>();
        mockStreamFactory.Setup(x => x.Create(It.IsAny<long?>())).Returns(() => new MemoryStream());

        mockServiceBusSender = new Mock<IServiceBusMessageSender>();
        mockServiceBusSender.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<OutboundMessage>(), It.IsAny<MessageSendSettings?>()))
            .ReturnsAsync(Guid.NewGuid().ToString());

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext ?? CreateMockHttpContext());

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

        var constructorArgs = new ApiService.ApiServiceConstructorArgs(
            new RevolutionaryStuffService.RevolutionaryStuffServiceConstrutorArgs(mockLoggerFactory.Object)
        );

        return new WebhookAutoResponder<TestBlobWriter>(
            blobWriter,
            mockWebSessionArchiver.Object,
            mockStreamFactory.Object,
            mockServiceBusSender.Object,
            mockHttpContextAccessor.Object,
            Options.Create(config),
            constructorArgs
        );
    }

    private HttpContext CreateMockHttpContext(string method = "POST", string contentType = "application/json", Stream? body = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.ContentType = contentType;
        context.Request.Body = body ?? new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"test\":\"data\"}"));
        context.Request.ContentLength = context.Request.Body.Length;
        context.Response.Body = new MemoryStream();
        return context;
    }

    [TestMethod]
    public async Task GoAsync_DisabledService_Returns503()
    {
        var config = new WebhookAutoResponderConfig
        {
            BaseFolderName = "test",
            Services = new Dictionary<string, WebhookAutoResponderConfig.WebhookServiceConfig>
            {
                ["test-service"] = new() { Enabled = false, StorageFolderName = "test-folder" }
            }
        };

        var httpContext = CreateMockHttpContext();
        var responder = CreateResponder(config, out var blobWriter, out var mockServiceBusSender, httpContext);

        await ((IWebhookAutoResponder)responder).GoAsync("test-service");

        Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GoAsync_ServiceNotFound_Returns501()
    {
        var config = new WebhookAutoResponderConfig
        {
            BaseFolderName = "test",
            Services = new Dictionary<string, WebhookAutoResponderConfig.WebhookServiceConfig>()
        };

        var httpContext = CreateMockHttpContext();
        var responder = CreateResponder(config, out var blobWriter, out var mockServiceBusSender, httpContext);

        await ((IWebhookAutoResponder)responder).GoAsync("non-existent-service");

        Assert.AreEqual((int)HttpStatusCode.NotImplemented, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GoAsync_BasicAuthMissing_Returns401()
    {
        var config = new WebhookAutoResponderConfig
        {
            BaseFolderName = "test",
            Services = new Dictionary<string, WebhookAutoResponderConfig.WebhookServiceConfig>
            {
                ["test-service"] = new()
                {
                    Enabled = true,
                    StorageFolderName = "test-folder",
                    AuthenticationConfig = new()
                    {
                        BasicAuthUser = "user",
                        BasicAuthPass = "pass"
                    }
                }
            }
        };

        var httpContext = CreateMockHttpContext();
        var responder = CreateResponder(config, out var blobWriter, out var mockServiceBusSender, httpContext);

        await ((IWebhookAutoResponder)responder).GoAsync("test-service");

        Assert.AreEqual((int)HttpStatusCode.Unauthorized, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GoAsync_BasicAuthValid_Succeeds()
    {
        var config = new WebhookAutoResponderConfig
        {
            BaseFolderName = "test",
            Services = new Dictionary<string, WebhookAutoResponderConfig.WebhookServiceConfig>
            {
                ["test-service"] = new()
                {
                    Enabled = true,
                    StorageFolderName = "test-folder",
                    StoreRequest = false,
                    AuthenticationConfig = new()
                    {
                        BasicAuthUser = "user",
                        BasicAuthPass = "pass"
                    }
                }
            }
        };

        var httpContext = CreateMockHttpContext();
        var authHeader = WebHelpers.CreateBasicAuthorizationHeaderValue("user", "pass");
        httpContext.Request.Headers["Authorization"] = authHeader;

        var responder = CreateResponder(config, out var blobWriter, out var mockServiceBusSender, httpContext);

        await ((IWebhookAutoResponder)responder).GoAsync("test-service");

        Assert.AreEqual((int)HttpStatusCode.OK, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GoAsync_StoreRequestTrue_WritesBlob()
    {
        var config = new WebhookAutoResponderConfig
        {
            BaseFolderName = "base",
            Services = new Dictionary<string, WebhookAutoResponderConfig.WebhookServiceConfig>
            {
                ["test-service"] = new()
                {
                    Enabled = true,
                    StorageFolderName = "service-folder",
                    StoreRequest = true
                }
            }
        };

        var httpContext = CreateMockHttpContext();
        var responder = CreateResponder(config, httpContext, out var blobWriter, out var mockServiceBusSender);

        await ((IWebhookAutoResponder)responder).GoAsync("test-service");

        Assert.AreEqual(1, blobWriter.WrittenBlobs.Count);
        Assert.AreEqual("request.saz", blobWriter.WrittenBlobs[0].name);
        Assert.AreEqual("base/service-folder", blobWriter.WrittenBlobs[0].settings?.FolderHint);
    }

    [TestMethod]
    public async Task GoAsync_ErrorOccurs_WritesBlobWithErrorSuffix()
    {
        var config = new WebhookAutoResponderConfig
        {
            BaseFolderName = "base",
            Services = new Dictionary<string, WebhookAutoResponderConfig.WebhookServiceConfig>
            {
                ["test-service"] = new()
                {
                    Enabled = false, // This will cause an error
                    StorageFolderName = "service-folder",
                    StoreRequest = false // Even though StoreRequest is false, errors should be logged
                }
            }
        };

        var httpContext = CreateMockHttpContext();
        var responder = CreateResponder(config, out var blobWriter, out var mockServiceBusSender, httpContext);

        await ((IWebhookAutoResponder)responder).GoAsync("test-service");

        Assert.AreEqual(1, blobWriter.WrittenBlobs.Count);
        Assert.AreEqual("request.error.saz", blobWriter.WrittenBlobs[0].name);
        Assert.AreEqual("base/service-folder", blobWriter.WrittenBlobs[0].settings?.FolderHint);
        Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GoAsync_WithTopic_SendsMessageToServiceBus()
    {
        var config = new WebhookAutoResponderConfig
        {
            BaseFolderName = "base",
            Services = new Dictionary<string, WebhookAutoResponderConfig.WebhookServiceConfig>
            {
                ["test-service"] = new()
                {
                    Enabled = true,
                    StorageFolderName = "service-folder",
                    StoreRequest = false,
                    Topic = "test-topic"
                }
            }
        };

        var httpContext = CreateMockHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.com");
        httpContext.Request.Path = "/webhook/test";

        var responder = CreateResponder(config, out var blobWriter, out var mockServiceBusSender, httpContext);

        await ((IWebhookAutoResponder)responder).GoAsync("test-service");

        mockServiceBusSender.Verify(
            x => x.SendAsync(
                "test-topic",
                It.Is<OutboundMessage>(om => om.ContentType == "application/json"),
                It.IsAny<MessageSendSettings?>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GoAsync_QueryStringAuth_ValidatesCorrectly()
    {
        var config = new WebhookAutoResponderConfig
        {
            BaseFolderName = "base",
            Services = new Dictionary<string, WebhookAutoResponderConfig.WebhookServiceConfig>
            {
                ["test-service"] = new()
                {
                    Enabled = true,
                    StorageFolderName = "service-folder",
                    StoreRequest = false,
                    AuthenticationConfig = new()
                    {
                        QueryStringParameterName = "token",
                        QueryStringParameterValue = "secret123"
                    }
                }
            }
        };

        var httpContext = CreateMockHttpContext();
        httpContext.Request.QueryString = new QueryString("?token=secret123");

        var responder = CreateResponder(config, out var blobWriter, out var mockServiceBusSender, httpContext);

        await ((IWebhookAutoResponder)responder).GoAsync("test-service");

        Assert.AreEqual((int)HttpStatusCode.OK, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GoAsync_QueryStringAuthInvalid_Returns401()
    {
        var config = new WebhookAutoResponderConfig
        {
            BaseFolderName = "base",
            Services = new Dictionary<string, WebhookAutoResponderConfig.WebhookServiceConfig>
            {
                ["test-service"] = new()
                {
                    Enabled = true,
                    StorageFolderName = "service-folder",
                    AuthenticationConfig = new()
                    {
                        QueryStringParameterName = "token",
                        QueryStringParameterValue = "secret123"
                    }
                }
            }
        };

        var httpContext = CreateMockHttpContext();
        httpContext.Request.QueryString = new QueryString("?token=wrongvalue");

        var responder = CreateResponder(config, out var blobWriter, out var mockServiceBusSender, httpContext);

        await ((IWebhookAutoResponder)responder).GoAsync("test-service");

        Assert.AreEqual((int)HttpStatusCode.Unauthorized, httpContext.Response.StatusCode);
    }
}
