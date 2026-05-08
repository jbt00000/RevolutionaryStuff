using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.ApiCore.Startup;

namespace RevolutionaryStuff.Applets.Webhooked;

public class WebhookAutoResponderProgram<TBlobWriter> : ApiProgram
    where TBlobWriter : class, IWebhookedDiagnosticBlobWriter
{
    private readonly WebhookAutoResponderProgramSettings? Settings;

    public WebhookAutoResponderProgram(WebhookAutoResponderProgramSettings? settings = null)
    {
        Settings = settings;
    }

    protected override void MapWebEndpoints(WebApplication app)
    {
        base.MapWebEndpoints(app);
        app.MapWebhookAutoResponderWebEndpoints();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.UseRevolutionaryStuffWebhooked<TBlobWriter>(Settings?.WebhookedUseSettings);
        Settings?.ConfigureServices?.Invoke(services);
    }
}
