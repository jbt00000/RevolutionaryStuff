namespace RevolutionaryStuff.Core.Services.DependencyInjection;

[Obsolete("Use the ServiceProvider.GetServiceByName extension method instead", false)]
public class NamedTypeNamedFactory : INamedFactory
{
    private readonly IServiceProvider ServiceProvider;

    public NamedTypeNamedFactory(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        ServiceProvider = serviceProvider;
    }

    T INamedFactory.GetServiceByName<T>(string name)
        => ServiceProvider.GetServiceByName<T>(name);
}
