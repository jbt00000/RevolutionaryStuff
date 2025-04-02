using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.ApiCore.Middleware;
using RevolutionaryStuff.ApiCore.Services;
using RevolutionaryStuff.ApiCore.Services.HostedServices;
using RevolutionaryStuff.ApiCore.Services.PrincipalAccessors;
using RevolutionaryStuff.ApiCore.Services.ServerInfoFinders;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.ApiCore;

public static class Use
{
    public class Settings
    {
        public RevolutionaryStuff.Core.Use.Settings? RevolutionaryStuffCoreUseSettings { get; set; }
    }

    public static void UseRevolutionaryStuffApiCore(this IServiceCollection services, Settings? settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreUseSettings);
        services.AddSingleton<IServerInfoFinder, ServerInfoFinder>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IHttpContextClaimsPrincipalAccessor, HttpContextClaimsPrincipalAccessor>();
        services.ConfigureOptions<WebApiExceptionMiddleware.Config>(WebApiExceptionMiddleware.Config.ConfigSectionName);
        services.AddSingleton<BaseBackgroundService.BaseBackgroundServiceConstructorArgs>(); //Hosted services do NOT run under scoped contexts, thus this must also be registered as a singleton
        services.AddScoped<ApiService.ApiServiceConstructorArgs>();
    });
}
