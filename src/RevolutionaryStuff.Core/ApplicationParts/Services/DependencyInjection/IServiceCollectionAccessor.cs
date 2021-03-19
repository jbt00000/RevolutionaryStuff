using Microsoft.Extensions.DependencyInjection;

namespace RevolutionaryStuff.Core.ApplicationParts.Services.DependencyInjection
{
    public interface IServiceCollectionAccessor
    {
        IServiceCollection Services { get; }
    }
}
