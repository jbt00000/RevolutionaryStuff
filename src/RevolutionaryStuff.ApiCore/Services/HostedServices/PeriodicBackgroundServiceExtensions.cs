using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.ApiCore.Services.HostedServices;

public static class PeriodicBackgroundServiceExtensions
{
    public static void AddPeriodicHostedService<TRunner>(this IServiceCollection services, IConfiguration config, string configSectionName)
        where TRunner : class, IPeriodicServiceRunner
    {
        services.AddScoped<TRunner>();
        services.ConfigureOptions<PeriodicBackgroundServiceConfig>(configSectionName);
        services.AddHostedService(sp =>
        {
            var c = config.Get<PeriodicBackgroundServiceConfig>(configSectionName);
            IOptions<PeriodicBackgroundServiceConfig> co = new OptionsWrapper<PeriodicBackgroundServiceConfig>(c);
            var h = sp.Construct<PeriodicBackgroundService<TRunner>>(co);
            return h;
        });
    }
}

