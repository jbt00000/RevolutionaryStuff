using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;

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

    public static string GetConnectionString(this IServiceProvider sp, string connectionStringName)
        => sp.GetService<IConfiguration>().GetConnectionString(connectionStringName);

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

    public static void Substitute<TImp>(this IServiceCollection services, ServiceLifetime? newServiceLifetime = null, ServiceLifetime? existingServiceLifetime = null)
        => services.Substitute<TImp, TImp>(newServiceLifetime, existingServiceLifetime);

    public static T GetRequiredScopedService<T>(this IServiceProvider provider)
        => provider.CreateScope().ServiceProvider.GetRequiredService<T>();

    public static SERVICE_TYPE InstantiateServiceWithOverriddenDependencies<SERVICE_TYPE>(this IServiceProvider provider, ServiceCollection services, params object[] overriddenLoadedDependencies)
    {
        var serviceType = typeof(SERVICE_TYPE);
        Type implementationType = null;
        foreach (var sd in services)
        {
            if (sd.Lifetime == ServiceLifetime.Singleton) continue;
            if (!serviceType.IsA(sd.ServiceType)) continue;
            implementationType = sd.ImplementationType;
        }
        if (implementationType == null) throw new TypeLoadException($"Could not find a seervice description for {typeof(SERVICE_TYPE)}");

        return (SERVICE_TYPE)provider.Construct(implementationType, overriddenLoadedDependencies);
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
                Stuff.Noop();
            }
            var co = ci.Invoke(args.ToArray());
            return co;
NextConstructor:
            Stuff.Noop();
        }
        throw new TypeLoadException($"Could not find a workable constructor to instantiate {t}");
    }
}
