using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Core.Services.DependencyInjection;

public class NamedTypeNamedFactory : BaseLoggingDisposable, INamedFactory
{
    private readonly IServiceProvider ServiceProvider;
    private readonly IServiceCollectionAccessor ServiceCollectionAccessor;

    public NamedTypeNamedFactory(IServiceProvider serviceProvider, IServiceCollectionAccessor serviceCollectionAccessor, ILogger<NamedTypeNamedFactory> logger)
        : base(logger)
    {
        ServiceProvider = serviceProvider;
        ServiceCollectionAccessor = serviceCollectionAccessor;
    }

    T INamedFactory.GetServiceByName<T>(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var sd = ServiceCollectionAccessor.Services.First(
            z =>
            z.ServiceType.IsA<T>() &&
            (z.ImplementationType.GetCustomAttribute<NamedTypeAttribute>()?.Names ?? Empty.StringArray).Contains(name)
            );
        return (T)ServiceProvider.GetService(sd.ImplementationType);
    }
}
