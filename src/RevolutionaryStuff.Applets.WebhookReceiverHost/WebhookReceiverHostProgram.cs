using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.ApiCore.Startup;
using RevolutionaryStuff.Applets.Blobs;
using RevolutionaryStuff.Applets.WebhookReceiverHost.Services.WebhookAutoResponders;
using RevolutionaryStuff.AspNetCore;
using RevolutionaryStuff.Azure;
using RevolutionaryStuff.Storage;

namespace RevolutionaryStuff.Applets.WebhookReceiverHost;

public abstract class WebhookReceiverHostProgram(WebhookReceiverHostProgramSettings? Settings=null) : ApiProgram
{
    protected override void MapWebEndpoints(WebApplication app)
    {
        base.MapWebEndpoints(app);
        app.MapWebhookAutoResponderWebEndpoints();
    }

    protected abstract Func<BlobWriterHelpers.PathProviderArgs, string> GetWebhookAutoResponderBlobWriterPathProvider();

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.UseRevolutionaryStuffApplets(Settings?.RevolutionaryStuffAppletsUseSettings);
        services.UseRevolutionaryStuffAspNetCore(Settings?.AspNetCoreUseSettings);
        services.UseRevolutionaryStuffAzure(Settings?.AzureUseSettings);

        services.AddHttpContextAccessor();

        services.AddScoped<IWebhookAutoResponder, WebhookAutoResponder>();
        services.ConfigureOptions<WebhookAutoResponderConfig>(WebhookAutoResponderConfig.ConfigSectionName);
        services.AddBlobWriter<IStorageProvider, IWebhookAutoResponderBlobWriter>(GetWebhookAutoResponderBlobWriterPathProvider());
    }
}
