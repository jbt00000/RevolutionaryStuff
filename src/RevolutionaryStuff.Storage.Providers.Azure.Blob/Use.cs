using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Storage.Implementation;

namespace RevolutionaryStuff.Storage.Providers.Azure.Blob;

public static class Use
{
    /// <summary>
    /// Registers a scoped <typeparamref name="TIAzureBlobStorageProvider"/> (which must extend <see cref="IStorageProvider"/>)
    /// backed by an <see cref="AzureBlobStorageProvider"/> whose configuration is bound from
    /// <paramref name="configSectionName"/>.
    /// </summary>
    /// <typeparam name="TIAzureBlobStorageProvider">The typed marker interface for this storage provider instance.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configSectionName">The configuration section name to bind <see cref="AzureBlobStorageProvider.Config"/> from.</param>
    public static IServiceCollection AddTypedAzureBlobStorageProvider<TIAzureBlobStorageProvider>(
        this IServiceCollection services,
        string configSectionName,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TIAzureBlobStorageProvider : class, IAzureBlobStorageProvider
        => services.AddTypedStorageProvider<TIAzureBlobStorageProvider, AzureBlobStorageProvider, AzureBlobStorageProvider.Config>(configSectionName, lifetime);
}
