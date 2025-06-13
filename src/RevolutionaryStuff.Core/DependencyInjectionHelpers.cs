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
    public static void ConfigureOptions<TOptions>(this IServiceCollection services, string sectionName, bool registerIPostConfigure = true) where TOptions : class
    {
        services.AddOptions<TOptions>()
                        .Configure<IConfiguration>((settings, configuration) =>
                        {
                            configuration.GetSection(sectionName).Bind(settings);
                        });
        if (registerIPostConfigure && typeof(TOptions).IsA<IPostConfigure>())
        {
            services.PostConfigure<TOptions>(o => ((IPostConfigure)o).PostConfigure());
        }
    }

    public static string GetConnectionString(this IServiceProvider serviceProvider, string connectionStringName)
        => serviceProvider.GetService<IConfiguration>().GetConnectionString(connectionStringName);

    public static void Substitute<TInt, TImp>(this IServiceCollection services, ServiceLifetime? newServiceLifetime = null, ServiceLifetime? existingServiceLifetime = null)
    {
        var serviceDescriptors = services.Where(s => typeof(TInt).IsA(s.ServiceType) && (existingServiceLifetime == null || s.Lifetime == existingServiceLifetime.Value)).ToList();
        Requires.Positive(serviceDescriptors.Count, nameof(serviceDescriptors));
        foreach (var oldServiceDescriptor in serviceDescriptors)
        {
            services.Remove(oldServiceDescriptor);
            var newServiceDescriptor = new ServiceDescriptor(oldServiceDescriptor.ServiceType, typeof(TImp), newServiceLifetime.GetValueOrDefault(oldServiceDescriptor.Lifetime));
            services.Add(newServiceDescriptor);
        }
    }

    public static void AddIndirect<TInt, TImp>(this IServiceCollection services)
    {
        var tTImp = typeof(TImp);
        Requires.True(tTImp.IsInterface);
        var impDescriptor = services.Where(s => s.ServiceType.IsA<TImp>()).Single();
        var newServiceDescriptor = new ServiceDescriptor(typeof(TInt), sp => sp.GetRequiredService<TImp>(), impDescriptor.Lifetime);
        services.Add(newServiceDescriptor);
    }

    public static void Substitute<TImp>(this IServiceCollection services, ServiceLifetime? newServiceLifetime = null, ServiceLifetime? existingServiceLifetime = null)
        => services.Substitute<TImp, TImp>(newServiceLifetime, existingServiceLifetime);

    public static SERVICE_TYPE InstantiateServiceWithOverriddenDependencies<SERVICE_TYPE>(this IServiceProvider serviceProvider, IServiceCollection services, params object[] overriddenLoadedDependencies)
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
            : (SERVICE_TYPE)serviceProvider.Construct(implementationType, overriddenLoadedDependencies);
    }

    public static T Construct<T>(this IServiceProvider provider, params object[] overriddenLoadedDependencies)
        => (T)provider.Construct(typeof(T), overriddenLoadedDependencies);

    public static object Construct(this IServiceProvider provider, Type t, params object[] overriddenLoadedDependencies)
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
                var o = provider.GetService(paramType);
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

    public static void ConfigureTenantedOptions<TTenantFinder, TOptions>(this IServiceCollection services, string sectionName)
        where TTenantFinder : ITenantIdProvider
        where TOptions : class, new()
    {
        services.ConfigureOptions<TenantedConfig<TOptions>>(sectionName);
        services.AddScoped<IOptions<TOptions>, TenantedOptionsWrapper<TTenantFinder, TOptions>>();
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

    public static TService GetServiceByName<TService>(this IServiceProvider serviceProvider, string serviceName)
    {
        Requires.Text(serviceName);

        TService service = default;

        var serviceCollectionAccessor = serviceProvider.GetRequiredService<IServiceCollectionAccessor>();

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
                ? (TService)serviceProvider.GetService(sd.ServiceType)
                : (TService)serviceProvider.GetService(sd.ImplementationType);
        }

        return service;
    }
}
