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

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.UseRevolutionaryStuffWebhooked<TBlobWriter>(Settings?.WebhookedUseSettings);
        Settings?.ConfigureServices?.Invoke(services);
    }
}
