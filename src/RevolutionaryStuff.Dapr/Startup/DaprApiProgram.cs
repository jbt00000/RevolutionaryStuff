using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.ApiCore.Startup;

namespace RevolutionaryStuff.Dapr.Startup;

public abstract class DaprApiProgram(DaprApiProgram.Settings _Settings) : ApiProgram
{
    public record Settings(bool EnablePubSubSubscriptions)
    { }

    protected override void MapWebEndpoints(WebApplication app)
    {
        base.MapWebEndpoints(app);

        if (_Settings.EnablePubSubSubscriptions)
        {

            // Dapr will send serialized event object vs. being raw CloudEvent
            app.UseCloudEvents();

            // needed for Dapr pub/sub routing
            app.MapSubscribeHandler();
        }
    }

    protected override void SetupConfigurationForRemoteSecrets(WebApplicationBuilder builder)
    {
        base.SetupConfigurationForRemoteSecrets(builder);
        //builder.AddDaprSecrets();
    }

    protected override void UseHttpsRedirection(WebApplication app)
    {
        //DO NOT CALL THE BASE!!!
        //Dapr only works over HTTP (unless explicitly configured for HTTPS). If your Web API is forcing HTTPS, Dapr will receive a 307 redirect, causing the issue.
        Stuff.NoOp();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.UseRevolutionaryStuffDapr();
    }
}
