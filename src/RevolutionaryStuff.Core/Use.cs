using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.ApplicationParts.TextTemplates;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Services.CodeStringGenerator;
using RevolutionaryStuff.Core.Services.Correlation;
using RevolutionaryStuff.Core.Services.DependencyInjection;
using RevolutionaryStuff.Core.Services.Http;
using RevolutionaryStuff.Core.Services.TemporaryStreamFactory;
using RevolutionaryStuff.Core.Services.Tenant;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Core;

public static class Use
{
    public class Settings
    {
        public string BuilderConfigConfigSectionName { get; set; }

        public string RevolutionaryStuffCoreConfigSectionName { get; set; }

        public string TemporaryStreamFactoryConfigSectionName { get; set; }

        public string ConfiguredTenantIdProviderConfigSectionName { get; set; }
    }

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffCore(Settings settings = null)
                => sources.Use(
                    settings,
                    () =>
            {
                sources.AddHttpClient();

                sources.AddSingleton<ILocalCacher>(Cache.DataCacher);

                sources.ConfigureOptions<RevolutionaryStuffCoreConfig>(settings?.RevolutionaryStuffCoreConfigSectionName ?? RevolutionaryStuffCoreConfig.ConfigSectionName);

                sources.ConfigureOptions<RsllcTemporaryStreamFactory.Config>(settings?.TemporaryStreamFactoryConfigSectionName ?? RsllcTemporaryStreamFactory.Config.ConfigSectionName);
                sources.AddSingleton<ITemporaryStreamFactory, RsllcTemporaryStreamFactory>();

                sources.AddSingleton<IServiceCollectionAccessor>(new HardcodedServiceCollectionProvider(sources));
                sources.AddScoped<INamedFactory, NamedTypeNamedFactory>();


                sources.ConfigureOptions<CorrelationIdFactory.Config>(CorrelationIdFactory.Config.ConfigSectionName);
                sources.AddSingleton<ICorrelationIdFactory, CorrelationIdFactory>();
                sources.AddScoped<ICorrectionIdFindOrCreate, CorrectionIdFindOrCreate>();
                sources.AddScoped<ICorrelationIdFinder, HardcodedCorrelationIdFinder>();
                sources.AddScoped<HardcodedCorrelationIdFinder>(); //so this can simply be asked for

                sources.AddSingleton<IConnectionStringProvider, ServiceProviderConnectionStringProvider>();

                sources.AddScoped<IHttpMessageSender, HttpClientHttpMessageSender>();

                sources.AddSingleton(IJsonSerializer.Default);

                sources.ConfigureOptions<BuilderConfig>(settings?.BuilderConfigConfigSectionName ?? BuilderConfig.ConfigSectionName);

                #region Tenant
                sources.ConfigureOptions<ConfiguredTenantIdProvider.Config>(settings?.ConfiguredTenantIdProviderConfigSectionName ?? ConfiguredTenantIdProvider.Config.ConfigSectionName);
                sources.AddScoped<IConfiguredTenantIdProvider, ConfiguredTenantIdProvider>();
                sources.AddScoped<IProgrammaticTenantIdProvider, ProgrammaticTenantIdProvider>();

                sources.ConfigureOptions<ConfiguredTenantIdEnumerator.Config>(ConfiguredTenantIdEnumerator.Config.ConfigSectionName);
                sources.AddSingleton<IConfiguredTenantIdEnumerator, ConfiguredTenantIdEnumerator>();
                #endregion

                #region Services
                sources.AddScoped<RevolutionaryStuffService.RevolutionaryStuffServiceConstrutorArgs>();
                sources.AddSingleton<ICodeStringGenerator, DefaultCodeStringGenerator>();
                #endregion
            });
        public IServiceCollection AddTextTemplateRenderer<TITextTemplateRenderer, TImplementation>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TITextTemplateRenderer : ITextTemplateRenderer
            where TImplementation : class, TITextTemplateRenderer
        {
            sources.Add(new ServiceDescriptor(typeof(TITextTemplateRenderer), typeof(TImplementation), serviceLifetime));
            if (typeof(TITextTemplateRenderer) != typeof(ITextTemplateRenderer))
            {
                sources.Add(new ServiceDescriptor(typeof(ITextTemplateRenderer), typeof(TImplementation), serviceLifetime));
            }
            return sources;
        }
    }


}
