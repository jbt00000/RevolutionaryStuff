using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Applets.WebhookReceiverHost;

namespace RevolutionaryStuff.Applets.WebhookReceiverHost;

public static class WebhookAutoResponderHelpers
{
    public static MapWebhookAutoResponderWebEndpointsSettings DefaultSettings { get; } = new(["WebHook"]);

    public static void MapWebhookAutoResponderWebEndpoints(this WebApplication app, MapWebhookAutoResponderWebEndpointsSettings? settings = null)
    {
        settings ??= DefaultSettings;
        var tags = settings.TagNames?.ToArray() ?? [];
        var config = app.Services.GetRequiredService<IOptions<WebhookAutoResponderConfig>>().Value;
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(WebhookAutoResponderHelpers));
        foreach (var service in config.Services.NullSafeEnumerable().Where(kvp => kvp.Value.Enabled))
        {
            var serviceConfig = service.Value;
            var pattern = serviceConfig.WebRoute ?? service.Key;
            var b = app.MapMethods(pattern, serviceConfig.AllowedMethods ?? [WebHelpers.Methods.Post], async (IWebhookAutoResponder webhookAutoResponder) =>
            {
                await webhookAutoResponder.GoAsync(service.Key);
            }).WithName(service.Key);
            if (tags.Length > 0)
            {
                b = b.WithTags(tags);
            }
            logger.LogInformation("Mapped webhook auto responder for service {ServiceName} with route {Route}", service.Key, pattern);
        }
    }

    public record MapWebhookAutoResponderWebEndpointsSettings(IList<string>? TagNames);
}
