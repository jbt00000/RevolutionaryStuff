using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.ApplicationParts.Services;
using RevolutionaryStuff.Core.ApplicationParts.Services.DependencyInjection;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core
{
    public static class _Use
    {
        public class Settings
        {
            public string RevolutionaryStuffCoreConfigSectionName { get; set; }
        }

        public static void UseRevolutionaryStuffCore(this IServiceCollection services, Settings settings=null)
        {
            services.AddSingleton<ILocalCacher>(Cache.DataCacher);

            services.ConfigureOptions<RevolutionaryStuffCoreConfig>(settings?.RevolutionaryStuffCoreConfigSectionName ?? RevolutionaryStuffCoreConfig.ConfigSectionName);

            services.AddSingleton<IServiceCollectionAccessor>(new HardcodedServiceCollectionProvider(services));
            services.AddScoped<INamedFactory, NamedTypeNamedFactory>();

            services.AddScoped<IHttpMessageSender, HttpClientHttpMessageSender>();
        }
    }
}
