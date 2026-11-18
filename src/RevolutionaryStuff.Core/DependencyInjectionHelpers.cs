using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Collections;
using RevolutionaryStuff.Core.Services.DependencyInjection;
using RevolutionaryStuff.Core.Services.Tenant;

namespace RevolutionaryStuff.Core;

public static class DependencyInjectionHelpers
{
    extension(IServiceCollection sources)
    {
        public IServiceCollection ConfigureOptions<TOptions>(string sectionName, bool registerIPostConfigure = true) where TOptions : class
        {
            sources.AddOptions<TOptions>()
                            .Configure<IConfiguration>((settings, configuration) =>
                            {
                                configuration.GetSection(sectionName).Bind(settings);
                            });
            if (registerIPostConfigure && typeof(TOptions).IsA<IPostConfigure>())
            {
                sources.PostConfigure<TOptions>(o => ((IPostConfigure)o).PostConfigure());
            }
            return sources;
        }
        public IServiceCollection Substitute<TInt, TImp>(ServiceLifetime? newServiceLifetime = null, ServiceLifetime? existingServiceLifetime = null)
        {
            var serviceDescriptors = sources.Where(s => typeof(TInt).IsA(s.ServiceType) && (existingServiceLifetime == null || s.Lifetime == existingServiceLifetime.Value)).ToList();
            Requires.Positive(serviceDescriptors.Count, nameof(serviceDescriptors));
            foreach (var oldServiceDescriptor in serviceDescriptors)
            {
                sources.Remove(oldServiceDescriptor);
                var newServiceDescriptor = new ServiceDescriptor(oldServiceDescriptor.ServiceType, typeof(TImp), newServiceLifetime.GetValueOrDefault(oldServiceDescriptor.Lifetime));
                sources.Add(newServiceDescriptor);
            }
            return sources;
        }
        public IServiceCollection AddIndirect<TInt, TImp>(ServiceLifetime fallbackServiceLifetime = ServiceLifetime.Scoped, bool removeExisting = true)
        {
            var tTInt = typeof(TInt);
            var tTImp = typeof(TImp);
            Requires.True(tTImp.IsInterface);

            if (removeExisting)
            {
                sources.Remove(z => z.ServiceType == tTInt);
            }

            var newServiceDescriptor = new ServiceDescriptor(typeof(TInt), sp =>
            {
                var ret = sp.GetRequiredService<TImp>();
                if (ret == null)
                {
                    throw new Exception($"Could not find required service implementation of {tTImp.Name}, so could not returna {tTInt.Name}");
                }
                return ret;
            }, fallbackServiceLifetime);

            sources.Add(newServiceDescriptor);
            return sources;
        }
        public IServiceCollection Substitute<TImp>(ServiceLifetime? newServiceLifetime = null, ServiceLifetime? existingServiceLifetime = null)
                => sources.Substitute<TImp, TImp>(newServiceLifetime, existingServiceLifetime);
        public IServiceCollection ConfigureTenantedOptions<TTenantFinder, TOptions>(string sectionName)
                where TTenantFinder : ITenantIdProvider
                where TOptions : class, new()
        {
            sources.ConfigureOptions<TenantedConfig<TOptions>>(sectionName);
            sources.AddScoped<IOptions<TOptions>, TenantedOptionsWrapper<TTenantFinder, TOptions>>();
            return sources;
        }
    }

    extension(IServiceProvider source)
    {
        public string GetConnectionString(string connectionStringName)
                => source.GetService<IConfiguration>().GetConnectionString(connectionStringName);
        public SERVICE_TYPE InstantiateServiceWithOverriddenDependencies<SERVICE_TYPE>(IServiceCollection services, params object[] overriddenLoadedDependencies)
        {
            var serviceType = typeof(SERVICE_TYPE);
            Type implementationType = null;
            foreach (var sd in services)
            {
                if (sd.Lifetime == ServiceLifetime.Singleton) continue;
                if (!serviceType.IsA(sd.ServiceType)) continue;
                implementationType = sd.ImplementationType;
            }
            return implementationType == null
                ? throw new TypeLoadException($"Could not find a service description for {typeof(SERVICE_TYPE)}")
                : (SERVICE_TYPE)source.Construct(implementationType, overriddenLoadedDependencies);
        }
        public T Construct<T>(params object[] overriddenLoadedDependencies)
                => (T)source.Construct(typeof(T), overriddenLoadedDependencies);
        public object Construct(Type t, params object[] overriddenLoadedDependencies)
        {
            foreach (var ci in t.GetConstructors().OrderByDescending(ci => ci.GetParameters().Length))
            {
                var args = new List<object>();
                foreach (var p in ci.GetParameters())
                {
                    var paramType = p.ParameterType;
                    foreach (var dep in overriddenLoadedDependencies)
                    {
                        if (dep != null && dep.GetType().IsA(paramType))
                        {
                            args.Add(dep);
                            goto NextParam;
                        }
                    }
                    var o = source.GetService(paramType);
                    if (o == null) goto NextConstructor;
                    args.Add(o);
NextParam:
                    Stuff.NoOp();
                }
                var co = ci.Invoke(args.ToArray());
                return co;
NextConstructor:
                Stuff.NoOp();
            }
            throw new TypeLoadException($"Could not find a workable constructor to instantiate {t}");
        }
        public TService GetServiceByName<TService>(string serviceName)
        {
            Requires.Text(serviceName);

            TService service = default;

            var serviceCollectionAccessor = source.GetRequiredService<IServiceCollectionAccessor>();

            lock (ImplementationTypesByServiceType)
            {
                if (ImplementationTypesByServiceType.Count == 0)
                {
                    serviceCollectionAccessor.Services.ForEach(sd => ImplementationTypesByServiceType.Add(sd.ServiceType, sd.ImplementationType));
                }
            }

            var sds = serviceCollectionAccessor.Services.Where(
                z =>
                z.ServiceType.IsA<TService>() &&
                (z.ImplementationType.GetCustomAttribute<NamedServiceAttribute>()?.ServiceNames ?? Empty.StringArray).Contains(serviceName)
                )
                .OrderBy(sd => ImplementationTypesByServiceType[sd.ServiceType].Count)
                .ToList();

            if (sds.Count > 0)
            {
                var sd = sds[0];
                service = ImplementationTypesByServiceType[sd.ServiceType].Count == 1
                    ? (TService)source.GetService(sd.ServiceType)
                    : (TService)source.GetService(sd.ImplementationType);
            }

            return service;
        }
    }















    private class TenantedConfig<TConfig>
    {
        public Dictionary<string, TConfig> Tenants { get; } = [];
    }

    private class TenantedOptionsWrapper<TTenantFinder, TOptions> : IOptions<TOptions>
        where TTenantFinder : ITenantIdProvider
        where TOptions : class, new()
    {
        private readonly ITenantIdProvider TenantProvider;
        private readonly IOptions<TenantedConfig<TOptions>> TenantedConfigOptions;

        public TenantedOptionsWrapper(TTenantFinder tenantFinder, IOptions<TenantedConfig<TOptions>> tenantedConfigOptions)
        {
            ArgumentNullException.ThrowIfNull(tenantFinder);
            ArgumentNullException.ThrowIfNull(tenantedConfigOptions);

            TenantProvider = tenantFinder;
            TenantedConfigOptions = tenantedConfigOptions;
        }

        TOptions IOptions<TOptions>.Value
        {
            get
            {
                var tid = TenantProvider.GetTenantId();
                var tenantedConfig = TenantedConfigOptions.Value;

                TOptions c;
                lock (tenantedConfig)
                {
                    if (!tenantedConfig.Tenants.TryGetValue(tid, out c))
                    {
                        const string defaultTenantId = "";
                        if (!tenantedConfig.Tenants.TryGetValue(defaultTenantId, out c))
                        {
                            c = new();
                            tenantedConfig.Tenants[defaultTenantId] = c;
                        }
                    }
                }

                ArgumentNullException.ThrowIfNull(c);

                return c;
            }
        }
    }


    private static readonly MultipleValueDictionary<Type, Type> ImplementationTypesByServiceType = [];




    extension(IHttpClientBuilder source)
    {
        public IHttpClientBuilder ConfigureHttpClientWithApiConfig<TApiConfig>(string configSectionName, Action<IServiceProvider, HttpClient, TApiConfig> configureClient = null)
            where TApiConfig : ApiConfig
        {
            source.Services.ConfigureOptions<TApiConfig>(configSectionName);
            return source.ConfigureHttpClient((sp, hc) =>
            {
                var config = sp.GetRequiredService<IOptions<TApiConfig>>().Value;
                hc.BaseAddress = new Uri(config.BaseAddress);
                switch (config.AuthorizationType)
                {
                    case ApiConfig.AuthorizationTypeEnum.Basic:
                        Requires.Text(config.AuthorizationBasicUsername);
                        Requires.Text(config.AuthorizationBasicPassword);
                        hc.DefaultRequestHeaders.AddBasicAuthorization(config.AuthorizationBasicUsername, config.AuthorizationBasicPassword);
                        break;
                    case ApiConfig.AuthorizationTypeEnum.Bearer:
                        throw new NotImplementedException("Bearer not yet supported");
                    case ApiConfig.AuthorizationTypeEnum.ApiKey:
                        Requires.Text(config.ApiKey);
                        hc.DefaultRequestHeaders.Authorization = new(config.ApiKey);
                        break;
                    case ApiConfig.AuthorizationTypeEnum.None:
                        break;
                    default:
                        if (config.CustomAuthorizationHeaderName != null && config.CustomAuthorizationHeaderValue != null)
                        {
                            hc.DefaultRequestHeaders.Add(config.CustomAuthorizationHeaderName, config.CustomAuthorizationHeaderValue);
                        }
                        break;
                }
                configureClient?.Invoke(sp, hc, config);
            });
        }
        public IHttpClientBuilder ConfigureHttpClientWithApiConfig(string configSectionName)
                => source.ConfigureHttpClientWithApiConfig<ApiConfig>(configSectionName);
    }



    public class ApiConfig : IValidate
    {
        public string BaseAddress { get; set; }

        public enum AuthorizationTypeEnum
        {
            None,
            Basic,
            Bearer,
            ApiKey
        }

        public AuthorizationTypeEnum AuthorizationType { get; set; } = AuthorizationTypeEnum.None;

        public string CustomAuthorizationHeaderName { get; set; }
        public string CustomAuthorizationHeaderValue { get; set; }

        public string ApiKey { get; set; }

        public string AuthorizationBasicUsername { get; set; }
        public string AuthorizationBasicPassword { get; set; }

        public virtual void Validate()
        {
            Requires.Url(BaseAddress);
        }
    }
}
