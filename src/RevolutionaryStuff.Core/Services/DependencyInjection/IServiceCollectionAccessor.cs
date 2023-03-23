using Microsoft.Extensions.DependencyInjection;

namespace RevolutionaryStuff.Core.Services.DependencyInjection;

public interface IServiceCollectionAccessor
{
    IServiceCollection Services { get; }
}
