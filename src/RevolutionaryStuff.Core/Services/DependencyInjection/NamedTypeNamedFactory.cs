using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.Collections;

namespace RevolutionaryStuff.Core.Services.DependencyInjection;

public class NamedTypeNamedFactory : BaseLoggingDisposable, INamedFactory
{
    private readonly IServiceProvider ServiceProvider;
    private readonly IServiceCollectionAccessor ServiceCollectionAccessor;
    private readonly MultipleValueDictionary<Type, Type> ImplementationTypesByServiceType = new();

    public NamedTypeNamedFactory(IServiceProvider serviceProvider, IServiceCollectionAccessor serviceCollectionAccessor, ILogger<NamedTypeNamedFactory> logger)
        : base(logger)
    {
        ServiceProvider = serviceProvider;
        ServiceCollectionAccessor = serviceCollectionAccessor;
    }


    T INamedFactory.GetServiceByName<T>(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        T service = default;

        lock (ImplementationTypesByServiceType)
        {
            if (ImplementationTypesByServiceType.Count == 0)
            {
                ServiceCollectionAccessor.Services.ForEach(sd => ImplementationTypesByServiceType.Add(sd.ServiceType, sd.ImplementationType));
            }
        }

        var sds = ServiceCollectionAccessor.Services.Where(
            z =>
            z.ServiceType.IsA<T>() &&
            (z.ImplementationType.GetCustomAttribute<NamedTypeAttribute>()?.Names ?? Empty.StringArray).Contains(name)
            )
            .OrderBy(sd=> ImplementationTypesByServiceType[sd.ServiceType].Count)
            .ToList();

        if (sds.Count > 0)
        {
            var sd = sds[0];
            if (ImplementationTypesByServiceType[sd.ServiceType].Count == 1)
            {
                service = (T)ServiceProvider.GetService(sd.ServiceType);
            }
            else
            { 
                service = (T)ServiceProvider.GetService(sd.ImplementationType);
            }
        }

        return service;
    }
}
