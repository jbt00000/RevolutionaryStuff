using Microsoft.Extensions.DependencyInjection;

namespace RevolutionaryStuff.Core.ApplicationParts.Services.DependencyInjection;

public class HardcodedServiceCollectionProvider : IServiceCollectionAccessor
{
    private readonly IServiceCollection ServiceCollection;

    public HardcodedServiceCollectionProvider(IServiceCollection serviceCollection)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ServiceCollection = serviceCollection;
    }

    IServiceCollection IServiceCollectionAccessor.Services => ServiceCollection;
}
