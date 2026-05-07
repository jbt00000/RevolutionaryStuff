using RevolutionaryStuff.ApiCore.Services.Tenant;
using RevolutionaryStuff.AspNetCore;
using RevolutionaryStuff.Core.Services.Tenant;
using Striclops.Services.Core.Services.Auditing;
using Striclops.Services.Core.Startup;
using Striclops.Webhooks.Services.WebhookAutoResponders;

namespace Striclops.Webhooks;

public class Program : StriclopsProgram
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.UseRevolutionaryStuffAspNetCore();

        services.AddIndirect<ISoftTenantIdProvider, IHttpTenantIdProvider>();
        services.AddIndirect<ITenantIdProvider, ISoftTenantIdProvider>();

        services.AddIndirect<ICallerAuditCreator, IHttpCallerAuditCreator>();

        services.UseWebhookAutoResponder();
    }

    protected override void MapWebEndpoints(WebApplication app)
    {
        base.MapWebEndpoints(app);
        app.MapWebhookAutoResponderWebEndpoints();
    }

    public static Task Main(string[] args)
        => new Program().GoAsync(args);
}
