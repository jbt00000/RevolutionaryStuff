using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Applets.WebhookReceiverHost.Services.WebhookAutoResponders;
using RevolutionaryStuff.AspNetCore;
using RevolutionaryStuff.Azure;
using RevolutionaryStuff.Core.ApplicationParts;
using Striclops.Services.Core.Services.Storage;
using RevolutionaryStuff.Applets.Blobs;

namespace RevolutionaryStuff.Applets.WebhookReceiverHost;

public static class Use
{
    public class Settings
    {
        public Applets.Use.Settings? RevolutionaryStuffAppletsUseSettings { get; set; }
//        public AspNetCore.Use.Settings? AspNetCoreUseSettings { get; set; }
        public Azure.Use.Settings? AzureUseSettings { get; set; }
    }

    public static IServiceCollection UseRevolutionaryStuffWebhookReceiverHost(this IServiceCollection services, Func<BlobWriterHelpers.PathProviderArgs, string> webhookAutoResponderBlobWriterPathProvider, Settings? settings=null)
        => services.Use(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffApplets(settings?.RevolutionaryStuffAppletsUseSettings);
  //      services.UseRevolutionaryStuffAspNetCore(settings?.AspNetCoreUseSettings);
        services.UseRevolutionaryStuffAzure(settings?.AzureUseSettings);

        services.AddHttpContextAccessor();

        services.AddScoped<IWebhookAutoResponder, WebhookAutoResponder>();
        services.ConfigureOptions<WebhookAutoResponderConfig>(WebhookAutoResponderConfig.ConfigSectionName);
        services.AddBlobWriter<IDiagnosticServicesStorageProvider, IWebhookAutoResponderBlobWriter>(webhookAutoResponderBlobWriterPathProvider);
    });
}
