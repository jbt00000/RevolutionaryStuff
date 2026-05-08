using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.AspNetCore;
using RevolutionaryStuff.Azure;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Applets.Webhooked;

public static class Use
{
    public class Settings
    {
        public RevolutionaryStuff.Applets.Use.Settings? RevolutionaryStuffAppletsUseSettings { get; set; }
        public RevolutionaryStuff.AspNetCore.Use.Settings? AspNetCoreUseSettings { get; set; }
        public RevolutionaryStuff.Azure.Use.Settings? AzureUseSettings { get; set; }
    }

    public static IServiceCollection UseRevolutionaryStuffWebhooked<TBlobWriter>(this IServiceCollection services, Settings? settings = null)
        where TBlobWriter : class, IWebhookedDiagnosticBlobWriter
        => services.Use(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffApplets(settings?.RevolutionaryStuffAppletsUseSettings);
        services.UseRevolutionaryStuffAspNetCore(settings?.AspNetCoreUseSettings);
        services.UseRevolutionaryStuffAzure(settings?.AzureUseSettings);

        services.AddHttpContextAccessor();

        services.AddScoped<TBlobWriter>();
        services.AddScoped<IWebhookedDiagnosticBlobWriter>(sp => sp.GetRequiredService<TBlobWriter>());

        services.AddScoped<IWebhookAutoResponder, WebhookAutoResponder<TBlobWriter>>();

        services.ConfigureOptions<WebhookAutoResponderConfig>(WebhookAutoResponderConfig.ConfigSectionName);
    });
}
