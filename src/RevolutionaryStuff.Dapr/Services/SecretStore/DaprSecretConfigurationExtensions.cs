using Microsoft.Extensions.Configuration;

namespace RevolutionaryStuff.Dapr.Services.SecretStore;

public static class DaprSecretConfigurationExtensions
{
    public static IConfigurationBuilder AddDaprSecretStore(this IConfigurationBuilder builder, string storeName)
    {
        builder.Add(new DaprSecretConfigurationSource(storeName));
        return builder;
    }
}
