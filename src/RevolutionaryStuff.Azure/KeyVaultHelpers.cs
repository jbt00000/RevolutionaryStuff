using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Azure;
public static class KeyVaultHelpers
{
    public class SecretClientAuthenticationSettings : IValidate
    {
        public string KeyVaultConnectionString { get; }
        public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;

        public void Validate()
            => ExceptionHelpers.AggregateExceptionsAndReThrow(
                () => Requires.Text(KeyVaultConnectionString),
                () => Requires.True(AuthenticateWithWithDefaultAzureCredentials)
                );

        public SecretClientAuthenticationSettings() { }

        public SecretClientAuthenticationSettings(string keyVaultConnectionString, bool authenticateWithWithDefaultAzureCredentials = true)
        {
            Requires.Text(keyVaultConnectionString);

            KeyVaultConnectionString = keyVaultConnectionString;
            AuthenticateWithWithDefaultAzureCredentials = authenticateWithWithDefaultAzureCredentials;
        }
    }

    public static SecretClient CreateSecretClient(SecretClientAuthenticationSettings authenticationSettings)
    {
        Requires.Valid(authenticationSettings);

        var creds = new DefaultAzureCredential(new DefaultAzureCredentialOptions());
        return new SecretClient(new(authenticationSettings.KeyVaultConnectionString), creds);
    }
}
