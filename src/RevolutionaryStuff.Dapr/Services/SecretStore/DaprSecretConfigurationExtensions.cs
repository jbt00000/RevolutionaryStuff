using Microsoft.Extensions.Configuration;

namespace RevolutionaryStuff.Dapr.Services.SecretStore;

public static class DaprSecretConfigurationExtensions
{
    extension(IConfigurationBuilder source)
    {
        public IConfigurationBuilder AddDaprSecretStore(string storeName)
        {
            source.Add(new DaprSecretConfigurationSource(storeName));
            return source;
        }
    }
}
