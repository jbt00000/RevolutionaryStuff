using Microsoft.Extensions.Options;

namespace Striclops.Webhooks.Services.WebhookAutoResponders;

public static class WebhookAutoResponderHelpers
{
    public static void UseWebhookAutoResponder(this IServiceCollection services)
    {
        services.ConfigureOptions<WebhookAutoResponder.Config>(WebhookAutoResponder.Config.ConfigSectionName);
        services.AddScoped<IWebhookAutoResponder, WebhookAutoResponder>();
    }

    public static void MapWebhookAutoResponderWebEndpoints(this WebApplication app, MapWebhookAutoResponderWebEndpointsSettings settings = null)
    {
        settings ??= DefaultSettings;
        var tags = settings.TagNames?.ToArray() ?? [];
        var config = app.Services.GetRequiredService<IOptions<WebhookAutoResponder.Config>>().Value;
        var logger = app.Services.GetRequiredService<ILogger<WebhookAutoResponder>>();
        app.Configuration.Bind(WebhookAutoResponder.Config.ConfigSectionName, config);
        foreach (var service in config.Services.NullSafeEnumerable().Where(kvp => kvp.Value.Enabled))
        {
            var serviceConfig = service.Value;
            var pattern = serviceConfig.WebRoute ?? service.Key;
            var b = app.MapMethods(pattern, serviceConfig.AllowedMethods, async (IWebhookAutoResponder webhookAutoResponder) =>
                {
                    await webhookAutoResponder.GoAsync(service.Key);
                }).WithName(service.Key);
            if (tags.Length > 0)
            {
                b = b.WithTags(tags);
            }
            logger.LogInformation("Mapped webhook auto responder for service {serviceName} with route {route}", service.Key, pattern);
        }
    }

    public record MapWebhookAutoResponderWebEndpointsSettings(IList<string> TagNames)
    { }

    public static MapWebhookAutoResponderWebEndpointsSettings DefaultSettings { get; } = new(["WebHook"]);
}
