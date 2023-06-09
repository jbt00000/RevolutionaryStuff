using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Services.Correlation;
using RevolutionaryStuff.Core.Services.DependencyInjection;
using RevolutionaryStuff.Core.Services.Http;
using RevolutionaryStuff.Core.Services.TemporaryStreamFactory;

namespace RevolutionaryStuff.Core;

public static class _Use
{
    public class Settings
    {
        public string RevolutionaryStuffCoreConfigSectionName { get; set; }

        public string TemporaryStreamFactoryConfigSectionName { get; set; }
    }

    public static void UseRevolutionaryStuffCore(this IServiceCollection services, Settings settings = null)
    {
        services.AddHttpClient();

        services.AddSingleton<ILocalCacher>(Cache.DataCacher);

        services.ConfigureOptions<RevolutionaryStuffCoreConfig>(settings?.RevolutionaryStuffCoreConfigSectionName ?? RevolutionaryStuffCoreConfig.ConfigSectionName);

        services.ConfigureOptions<RsllcTemporaryStreamFactory.Config>(settings?.TemporaryStreamFactoryConfigSectionName ?? RsllcTemporaryStreamFactory.Config.ConfigSectionName);
        services.AddSingleton<ITemporaryStreamFactory, RsllcTemporaryStreamFactory>();

        services.AddSingleton<IServiceCollectionAccessor>(new HardcodedServiceCollectionProvider(services));
        services.AddScoped<INamedFactory, NamedTypeNamedFactory>();


        services.ConfigureOptions<CorrelationIdFactory.Config>(CorrelationIdFactory.Config.ConfigSectionName);
        services.AddSingleton<ICorrelationIdFactory, CorrelationIdFactory>();
        services.AddScoped<ICorrectionIdFindOrCreate, CorrectionIdFindOrCreate>();
        services.AddScoped<ICorrelationIdFinder, HardcodedCorrelationIdFinder>();
        services.AddScoped<ICorrelationIdFinder, HttpContextCorrelationIdFinder>();
        services.AddScoped<HardcodedCorrelationIdFinder>(); //so this can simply be asked for

        services.AddSingleton<IConnectionStringProvider, ServiceProviderConnectionStringProvider>();

        services.AddScoped<IHttpMessageSender, HttpClientHttpMessageSender>();
    }
}
