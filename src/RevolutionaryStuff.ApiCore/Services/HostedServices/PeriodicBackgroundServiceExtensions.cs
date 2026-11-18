using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.ApiCore.Services.HostedServices;

public static class PeriodicBackgroundServiceExtensions
{
    extension(IServiceCollection sources)
    {
        public IServiceCollection AddPeriodicHostedService<TRunner>(IConfiguration config, string configSectionName)
                where TRunner : class, IPeriodicServiceRunner
        {
            sources.AddScoped<TRunner>();
            sources.ConfigureOptions<PeriodicBackgroundServiceConfig>(configSectionName);
            sources.AddHostedService(sp =>
            {
                var c = config.Get<PeriodicBackgroundServiceConfig>(configSectionName);
                IOptions<PeriodicBackgroundServiceConfig> co = new OptionsWrapper<PeriodicBackgroundServiceConfig>(c);
                var h = sp.Construct<PeriodicBackgroundService<TRunner>>(co);
                return h;
            });
            return sources;
        }
    }
}

