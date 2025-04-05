using Microsoft.Extensions.Configuration;

namespace RevolutionaryStuff.Dapr.Services.SecretStore;
public class DaprSecretConfigurationSource(string StoreName) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new DaprSecretConfigurationProvider(StoreName);
}
