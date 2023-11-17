﻿using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Services.CodeStringGenerator;
using RevolutionaryStuff.Core.Services.Correlation;
using RevolutionaryStuff.Core.Services.DependencyInjection;
using RevolutionaryStuff.Core.Services.Http;
using RevolutionaryStuff.Core.Services.TemporaryStreamFactory;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Core;

#pragma warning disable IDE1006 // Naming Styles
public static class _Use
#pragma warning restore IDE1006 // Naming Styles
{
    public class Settings
    {
        public string RevolutionaryStuffCoreConfigSectionName { get; set; }

        public string TemporaryStreamFactoryConfigSectionName { get; set; }
    }

    public static void UseRevolutionaryStuffCore(this IServiceCollection services, Settings settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
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
        services.AddScoped<HardcodedCorrelationIdFinder>(); //so this can simply be asked for

        services.AddSingleton<IConnectionStringProvider, ServiceProviderConnectionStringProvider>();

        services.AddScoped<IHttpMessageSender, HttpClientHttpMessageSender>();

        services.AddSingleton(IJsonSerializer.Default);

        #region Services
        services.AddSingleton<ICodeStringGenerator, DefaultCodeStringGenerator>();
        #endregion
    });
}
