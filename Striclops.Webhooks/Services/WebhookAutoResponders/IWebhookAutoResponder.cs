using RevolutionaryStuff.Azure.Services.Messaging.Outbound;

namespace Striclops.Webhooks.Services.WebhookAutoResponders;

public interface IWebhookAutoResponder
{
    Task GoAsync(string serviceName, Func<WebhookAutoResponderWorkerArgs, Task<OutboundMessage>> workAsync = null);

    record WebhookAutoResponderWorkerArgs
    {
        public HttpContext Context { get; init; }
        public Stream DataStream { get; init; }
    }
}

