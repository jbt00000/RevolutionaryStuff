using System.IO;
using Microsoft.AspNetCore.Http;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound;

namespace RevolutionaryStuff.Applets.Webhooked;

public interface IWebhookAutoResponder
{
    Task GoAsync(string serviceName, Func<WebhookAutoResponderWorkerArgs, Task<OutboundMessage>>? workAsync = null);

    record WebhookAutoResponderWorkerArgs
    {
        public required HttpContext Context { get; init; }
        public required Stream DataStream { get; init; }
    }
}
