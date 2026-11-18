using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.ApiCore.Middleware;
using RevolutionaryStuff.ApiCore.Razor;
using RevolutionaryStuff.ApiCore.Services;
using RevolutionaryStuff.ApiCore.Services.HostedServices;
using RevolutionaryStuff.ApiCore.Services.PrincipalAccessors;
using RevolutionaryStuff.ApiCore.Services.ServerInfoFinders;
using RevolutionaryStuff.ApiCore.Services.Tenant;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.ApiCore;

public static class Use
{
    public class Settings
    {
        public RevolutionaryStuff.Core.Use.Settings? RevolutionaryStuffCoreUseSettings { get; set; }

        public string? HttpTenantIdProviderConfigSectionName { get; set; }
    }

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffApiCore(Settings? settings = null)
                => sources.Use(
                    settings,
                    () =>
            {
                sources.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreUseSettings);
                sources.AddSingleton<IServerInfoFinder, ServerInfoFinder>();
                sources.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

                sources.AddScoped<IHttpContextClaimsPrincipalAccessor, HttpContextClaimsPrincipalAccessor>();
                sources.AddScoped<ISoftClaimsPrincipalAccessor, SoftClaimsPrincipalAccessor>();

                sources.ConfigureOptions<WebApiExceptionMiddleware.Config>(WebApiExceptionMiddleware.Config.ConfigSectionName);

                sources.AddSingleton<BackgroundServiceBase.BackgroundServiceBaseConstructorArgs>(); //Hosted services do NOT run under scoped contexts, thus this must also be registered as a singleton
                sources.AddScoped<ApiService.ApiServiceConstructorArgs>();

                sources.ConfigureOptions<HttpTenantIdProvider.Config>(settings?.HttpTenantIdProviderConfigSectionName ?? HttpTenantIdProvider.Config.ConfigSectionName);
                sources.AddScoped<IHttpTenantIdProvider, HttpTenantIdProvider>();

                sources.AddScoped<RevolutionaryStuffPageModel.RevolutionaryStuffPageModelConstructorArgs>();
            });
    }
}
