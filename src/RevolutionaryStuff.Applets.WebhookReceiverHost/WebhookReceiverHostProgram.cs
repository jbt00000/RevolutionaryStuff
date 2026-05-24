using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.ApiCore.Startup;
using RevolutionaryStuff.Applets.Blobs;

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
        services.UseRevolutionaryStuffWebhookReceiverHost(GetWebhookAutoResponderBlobWriterPathProvider(), Settings?.WebhookReceiverHostUseSettings);
    }
}
