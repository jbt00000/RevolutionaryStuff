using Dapr.Client;
using Microsoft.Extensions.Configuration;

namespace RevolutionaryStuff.Dapr.Services.SecretStore;

public class DaprSecretConfigurationProvider(string StoreName) : ConfigurationProvider
{
    public override void Load()
        => LoadAsync().ExecuteSynchronously();

    private async Task LoadAsync()
    {
        using var client = new DaprClientBuilder().Build();
        var secrets = await client.GetBulkSecretAsync(StoreName);

        var data = new Dictionary<string, string?>();
        foreach (var secret in secrets)
        {
            foreach (var kvp in secret.Value)
            {
                data[kvp.Key] = kvp.Value;
            }
        }

        Data = data;
    }
}
