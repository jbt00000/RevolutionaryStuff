using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.ApiCore.Services;
using RevolutionaryStuff.Applets.Blobs;
using RevolutionaryStuff.AspNetCore.Services.SazGenerators;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus;
using RevolutionaryStuff.Core.Services.TemporaryStreamFactory;

namespace RevolutionaryStuff.Applets.WebhookReceiverHost;

internal class WebhookAutoResponder<TBlobWriter> : ApiService, IWebhookAutoResponder
    where TBlobWriter : IWebhookedDiagnosticBlobWriter
{
    private static class WebhookPropertyNames
    {
        public const string WebhookUrl = "webhookUrl";
        public const string WebhookServiceName = "webhookServiceName";
    }

    private readonly TBlobWriter BlobWriter;
    private readonly IWebSessionArchiver WebSessionArchiver;
    private readonly ITemporaryStreamFactory StreamFactory;
    private readonly IServiceBusMessageSender ServiceBusMessageSender;
    private readonly IHttpContextAccessor HttpContextAccessor;
    private readonly IOptions<WebhookAutoResponderConfig> ConfigOptions;

    public WebhookAutoResponder(
        TBlobWriter blobWriter,
        IWebSessionArchiver webSessionArchiver,
        ITemporaryStreamFactory streamFactory,
        IServiceBusMessageSender serviceBusMessageSender,
        IHttpContextAccessor httpContextAccessor,
        IOptions<WebhookAutoResponderConfig> configOptions,
        ApiService.ApiServiceConstructorArgs constructorArgs)
        : base(constructorArgs)
    {
        ArgumentNullException.ThrowIfNull(blobWriter);
        ArgumentNullException.ThrowIfNull(webSessionArchiver);
        ArgumentNullException.ThrowIfNull(streamFactory);
        ArgumentNullException.ThrowIfNull(serviceBusMessageSender);
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        ArgumentNullException.ThrowIfNull(configOptions);
        BlobWriter = blobWriter;
        WebSessionArchiver = webSessionArchiver;
        StreamFactory = streamFactory;
        ServiceBusMessageSender = serviceBusMessageSender;
        HttpContextAccessor = httpContextAccessor;
        ConfigOptions = configOptions;
    }

    async Task IWebhookAutoResponder.GoAsync(string serviceName, Func<IWebhookAutoResponder.WebhookAutoResponderWorkerArgs, Task<OutboundMessage>>? workAsync)
    {
        var config = ConfigOptions.Value;
        var context = HttpContextAccessor.HttpContext;
        ArgumentNullException.ThrowIfNull(context);
        ExceptionError? ee = null;
        var errorCode = 0;
        Stream? requestStream = null;
        var serviceConfig = config?.Services?.GetValueOrDefault(serviceName);

        using var serviceNameScopedProperty = LogScopedProperty("serviceName", serviceName);
        try
        {
            Requires.Text(serviceName);
            if (WebHelpers.Methods.IsPostOrPutOrPatch(context.Request.Method) && context.Request.Body != null)
            {
                requestStream = context.Request.ContentLength.HasValue ? StreamFactory.Create(context.Request.ContentLength.Value) : StreamFactory.Create();
                await context.Request.Body.CopyToAsync(requestStream);
                requestStream.Position = 0;
            }
            else
            {
                requestStream = new MemoryStream(0);
            }
            await OnHandleRequestAsync(context, requestStream, serviceConfig, serviceName, workAsync);
        }
        catch (HttpStatusCodeException hex)
        {
            LogException(hex);
            ee = new ExceptionError(hex);
            errorCode = (int)hex.Code;
        }
        catch (UnauthorizedAccessException uex)
        {
            LogException(uex);
            ee = new ExceptionError(uex);
            errorCode = (int)HttpStatusCode.Unauthorized;
        }
        catch (Exception ex)
        {
            LogException(ex);
            ee = new ExceptionError(ex);
            errorCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            if (serviceConfig?.StorageFolderName != null && (serviceConfig.StoreRequest || errorCode > 0))
            {
                requestStream.Position = 0;
                var blobName = $"request{(errorCode > 0 ? ".error" : "")}{WebSessionArchiver.FileExtension}";

                using var archiveStream = StreamFactory.Create();
                await WebSessionArchiver.GenerateAsync(new WebSession(context.Request, requestStream), archiveStream);
                archiveStream.Position = 0;

                var result = await BlobWriter.WriteBlobAsync(blobName, archiveStream, new WriteBlobSettings
                {
                    ContentType = MimeType.Application.OctetStream.PrimaryContentType,
                    FolderHint = $"{config.BaseFolderName}/{serviceConfig.StorageFolderName}"
                });
                LogTrace("Logged webhook request to {name} (size: {size})", result.Name, result.Size);
            }
            Stuff.Dispose(requestStream);
        }
        if (ee != null)
        {
            context.Response.StatusCode = errorCode;
            if (config?.RespondWithDetailedErrors == true)
            {
                await context.Response.WriteAsJsonAsync(ee);
            }
            else
            {
                await context.Response.WriteAsync(ee.ErrorMessage ?? "An error occurred");
            }
        }
    }

    protected virtual async Task OnHandleRequestAsync(HttpContext context, Stream requestStream, WebhookAutoResponderConfig.WebhookServiceConfig? serviceConfig, string serviceName, Func<IWebhookAutoResponder.WebhookAutoResponderWorkerArgs, Task<OutboundMessage>>? workAsync)
    {
        if (serviceConfig == null) throw new HttpStatusCodeException(HttpStatusCode.NotImplemented, $"Service {serviceName} not found");
        using var serviceConfigScopedProperty = LogScopedProperty("serviceConfig", serviceConfig, true);
        if (!serviceConfig.Enabled) throw new HttpStatusCodeException(HttpStatusCode.ServiceUnavailable, $"Service {serviceName} not enabled");
        var req = context.Request;
        var authConfig = serviceConfig.AuthenticationConfig;
        if (authConfig != null)
        {
            if (authConfig.BasicAuthUser != null && authConfig.BasicAuthPass != null)
            {
                var authValue = StringHelpers.TrimOrNull(req.Headers[WebHelpers.HeaderStrings.Authorization]);
                if (authValue == null) throw new UnauthorizedAccessException($"{WebHelpers.HeaderStrings.Authorization} Header was missing");
                if (authValue.Split(" ", true, out var scheme, out var secret))
                {
                    if (scheme.ToLower() != "basic")
                    {
                        throw new UnauthorizedAccessException($"Scheme [{scheme}] was passed in but only Basic is supported");
                    }
                    secret = secret.TrimOrNull();
                    if (secret != WebHelpers.CreateBasicAuthorizationHeaderValueParameter(authConfig.BasicAuthUser, authConfig.BasicAuthPass))
                    {
                        throw new UnauthorizedAccessException($"Inbound secret [{secret}] does not match expected value");
                    }
                }
            }
            if (authConfig.QueryStringParameterName != null)
            {
                var keyValues = req.Query[authConfig.QueryStringParameterName];
                if (keyValues.Count != 1)
                {
                    throw new UnauthorizedAccessException($"Received {keyValues.Count} occurrences of query string parameter {authConfig.QueryStringParameterName} but need exactly 1");
                }
                if (keyValues[0] != authConfig.QueryStringParameterValue)
                {
                    throw new UnauthorizedAccessException($"Received {authConfig.QueryStringParameterName} value of [{keyValues[0]}] which was not the expected one");
                }
            }
            if (authConfig.AcceptableContentType.HasData() && !MimeType.IsA(req.ContentType, authConfig.AcceptableContentType))
            {
                throw new HttpStatusCodeException(HttpStatusCode.UnprocessableContent, $"Expecting contentType {authConfig.AcceptableContentType} but received {context.Request.ContentType}");
            }
        }

        await OnHandleValidatedRequestAsync(context, requestStream, serviceConfig, serviceName, workAsync);
    }

    protected virtual async Task OnHandleValidatedRequestAsync(HttpContext context, Stream requestStream, WebhookAutoResponderConfig.WebhookServiceConfig serviceConfig, string serviceName, Func<IWebhookAutoResponder.WebhookAutoResponderWorkerArgs, Task<OutboundMessage>>? workAsync)
    {
        context.Response.StatusCode = (int)serviceConfig.SuccessCode;
        OutboundMessage? om = null;
        if (workAsync != null)
        {
            om = await workAsync(new IWebhookAutoResponder.WebhookAutoResponderWorkerArgs { Context = context, DataStream = requestStream });
        }
        if (serviceConfig.Topic != null)
        {
            if (om == null && requestStream != null)
            {
                om ??= OutboundMessage.Create(requestStream, context.Request.ContentType ?? MimeType.Application.OctetStream.PrimaryContentType);
            }
            if (om != null)
            {
                om.SetProperty(WebhookPropertyNames.WebhookUrl, context.Request.GetEncodedUrl());
                om.SetProperty(WebhookPropertyNames.WebhookServiceName, serviceName);
                await ServiceBusMessageSender.SendAsync(serviceConfig.Topic, om);
            }
        }
    }
}
