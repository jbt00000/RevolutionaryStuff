using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.AspNetCore.Services.SazGenerators;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus;
using RevolutionaryStuff.Core.Services.TemporaryStreamFactory;
using RevolutionaryStuff.Storage;
using Striclops.Services.Core.Services;
using Striclops.Services.Core.Services.Messaging;
using Striclops.Services.Core.Services.Storage;
using System.Net;

namespace Striclops.Webhooks.Services.WebhookAutoResponders;

internal class WebhookAutoResponder : StriclopsService, IWebhookAutoResponder
{
    private readonly IStorageProvider StorageProvider;
    private readonly IWebSessionArchiver WebSessionArchiver;
    private readonly ITemporaryStreamFactory StreamFactory;
    private readonly IServiceBusMessageSender ServiceBusMessageSender;
    private readonly IHttpContextAccessor HttpContextAccessor;
    private readonly IOptions<Config> ConfigOptions;

    public class Config
    {
        public const string ConfigSectionName = "WebhookResponder";

        public string BaseFolderName { get; set; }

        public class WebhookAuthenticationConfig
        {
            public string BasicAuthUser { get; set; }
            public string BasicAuthPass { get; set; }
            public string QueryStringParameterName { get; set; }
            public string QueryStringParameterValue { get; set; }
            public string AcceptableContentType { get; set; }
        }

        public class WebhookServiceConfig
        {
            /// <summary>
            /// When true, this endpoint should be enabled, else false
            /// </summary>
            public bool Enabled { get; set; }

            /// <summary>
            /// When true, store the request to the blob storage, also requires StorageFolderName to be set
            /// </summary>
            public bool StoreRequest { get; set; } = true;

            /// <summary>
            /// Name of the storage folder to use when storing the request
            /// </summary>
            public string StorageFolderName { get; set; }

            /// <summary>
            /// The http status code to respond with if this was a successful request
            /// </summary>
            public HttpStatusCode SuccessCode { get; set; } = HttpStatusCode.OK;

            /// <summary>
            /// Service Bus topic in which to send the payload
            /// </summary>
            public string Topic { get; set; }

            public WebhookAuthenticationConfig AuthenticationConfig { get; set; }

            public string WebRoute { get; set; }
            public IList<string> AllowedMethods { get; set; } = [WebHelpers.Methods.Post];
        }
        public bool RespondWithDetailedErrors { get; set; }

        public Dictionary<string, WebhookServiceConfig> Services { get; set; }
    }

    public WebhookAutoResponder(IDiagnosticServicesStorageProvider storageProvider, IWebSessionArchiver sazGenerator, ITemporaryStreamFactory streamFactory, IServiceBusMessageSender serviceBusMessageSender, IHttpContextAccessor httpContextAccessor, IOptions<Config> configOptions, StriclopsBackendServiceConstructorArgs constructorArgs)
        : base(constructorArgs)
    {
        ArgumentNullException.ThrowIfNull(storageProvider);
        ArgumentNullException.ThrowIfNull(sazGenerator);
        ArgumentNullException.ThrowIfNull(streamFactory);
        ArgumentNullException.ThrowIfNull(serviceBusMessageSender);
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        ArgumentNullException.ThrowIfNull(configOptions);
        StorageProvider = storageProvider;
        WebSessionArchiver = sazGenerator;
        StreamFactory = streamFactory;
        ServiceBusMessageSender = serviceBusMessageSender;
        HttpContextAccessor = httpContextAccessor;
        ConfigOptions = configOptions;
    }

    async Task IWebhookAutoResponder.GoAsync(string serviceName, Func<IWebhookAutoResponder.WebhookAutoResponderWorkerArgs, Task<OutboundMessage>> workAsync)
    {
        var config = ConfigOptions.Value;
        var context = HttpContextAccessor.HttpContext;
        ExceptionError ee = null;
        var errorCode = 0;
        Stream requestStream = null;
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
                var name = $"request{(errorCode > 0 ? ".error" : "")}";
                var archivePath = await StorageProvider.SaveStreamToDateHierarchicalPathWithUniqueNameAsync($"{config.BaseFolderName}/{serviceConfig.StorageFolderName}", name, WebSessionArchiver.FileExtension, ssa => WebSessionArchiver.GenerateAsync(new WebSession(context.Request, requestStream), ssa.Stream));
                LogTrace("Logged webhook request to {path}", serviceName, archivePath);
            }
            Stuff.Dispose(requestStream);
        }
        if (ee != null)
        {
            context.Response.StatusCode = errorCode;
            if (config.RespondWithDetailedErrors)
            {
                await context.Response.WriteAsJsonAsync(ee);
            }
            else
            {
                await context.Response.WriteAsync(ee.ErrorMessage);
            }
        }
    }

    protected virtual async Task OnHandleRequestAsync(HttpContext context, Stream requestStream, Config.WebhookServiceConfig serviceConfig, string serviceName, Func<IWebhookAutoResponder.WebhookAutoResponderWorkerArgs, Task<OutboundMessage>> workAsync)
    {
        if (serviceConfig == null) throw new HttpStatusCodeException(HttpStatusCode.NotImplemented, $"Service {serviceName} not found");
        using var serviceConfigScopedProperty = LogScopedProperty("serviceConfig", serviceConfig, true);
        if (!serviceConfig.Enabled) throw new HttpStatusCodeException(HttpStatusCode.ServiceUnavailable, $"Service {serviceName} not enabled");
        var req = context.Request;
        var authConfig = serviceConfig.AuthenticationConfig;
        if (authConfig != null)
        {
            if (authConfig.BasicAuthUser != null && authConfig.BasicAuthUser != null)
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

    protected virtual async Task OnHandleValidatedRequestAsync(HttpContext context, Stream requestStream, Config.WebhookServiceConfig serviceConfig, string serviceName, Func<IWebhookAutoResponder.WebhookAutoResponderWorkerArgs, Task<OutboundMessage>> workAsync)
    {
        context.Response.StatusCode = (int)serviceConfig.SuccessCode;
        OutboundMessage om = null;
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
                om.SetProperty(MessagingHelpers.UserPropertyNames.WebhookUrl, context.Request.GetEncodedUrl());
                om.SetProperty(MessagingHelpers.UserPropertyNames.WebhookServiceName, serviceName);
                await ServiceBusMessageSender.SendAsync(serviceConfig.Topic, om);
            }
        }
    }
}
