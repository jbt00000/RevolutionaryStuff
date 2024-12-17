using Azure.Security.KeyVault.Secrets;
using RevolutionaryStuff.Azure.Services.Authentication;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Azure;
public static class KeyVaultHelpers
{
    public class SecretClientAuthenticationSettings : IValidate
    {
        public string KeyVaultUrl { get; }
        public IAzureTokenCredentialProvider AzureTokenCredentialProvider { get; set; }

        public void Validate()
            => ExceptionHelpers.AggregateExceptionsAndReThrow(
                () => Requires.Text(KeyVaultUrl)
                );

        public SecretClientAuthenticationSettings() { }

        public SecretClientAuthenticationSettings(string keyVaultUrl, IAzureTokenCredentialProvider azureTokenCredentialProvider)
        {
            Requires.Text(keyVaultUrl);
            ArgumentNullException.ThrowIfNull(azureTokenCredentialProvider);

            KeyVaultUrl = keyVaultUrl;
            AzureTokenCredentialProvider = azureTokenCredentialProvider;
        }
    }

    public static SecretClient CreateSecretClient(SecretClientAuthenticationSettings authenticationSettings)
    {
        Requires.Valid(authenticationSettings);

        var creds = authenticationSettings.AzureTokenCredentialProvider.GetTokenCredential();
        return new SecretClient(new(authenticationSettings.KeyVaultUrl), creds);
    }
}
