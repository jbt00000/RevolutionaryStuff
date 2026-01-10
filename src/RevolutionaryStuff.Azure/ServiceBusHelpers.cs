using Azure.Messaging.ServiceBus;
using RevolutionaryStuff.Azure.Services.Authentication;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Azure;

public class ServiceBusHelpers
{
    public class ServiceBusClientAuthenticationSettings : IValidate
    {
        public string ServiceBusConnectionStringOrFullyQualifiedName { get; }
        public IAzureTokenCredentialProvider AzureTokenCredentialProvider { get; set; }
        public bool AuthenticateWithWithDefaultAzureCredentials { get; set; }

        public void Validate()
            => ExceptionHelpers.AggregateExceptionsAndReThrow(
                () => Requires.Text(ServiceBusConnectionStringOrFullyQualifiedName),
                () => Requires.True(!AuthenticateWithWithDefaultAzureCredentials || AzureTokenCredentialProvider != null)
                );

        public ServiceBusClientAuthenticationSettings() { }

        public ServiceBusClientAuthenticationSettings(string serviceBusConnectionStringOrFullyQualifiedName, IAzureTokenCredentialProvider azureTokenCredentialProvider, bool? authenticateWithWithDefaultAzureCredentials = null)
        {
            Requires.Text(serviceBusConnectionStringOrFullyQualifiedName);

            ServiceBusConnectionStringOrFullyQualifiedName = serviceBusConnectionStringOrFullyQualifiedName;
            AuthenticateWithWithDefaultAzureCredentials = authenticateWithWithDefaultAzureCredentials ?? (AzureTokenCredentialProvider != null);
            AzureTokenCredentialProvider = azureTokenCredentialProvider;
        }
    }

    public static ServiceBusClient ConstructServiceBusClient(ServiceBusClientAuthenticationSettings authenticationSettings, ServiceBusClientOptions options = null)
    {
        Requires.Valid(authenticationSettings);
        options ??= new();

        ServiceBusClient client;
        if (authenticationSettings.AuthenticateWithWithDefaultAzureCredentials)
        {
            var creds = authenticationSettings.AzureTokenCredentialProvider.GetTokenCredential();
            client = new ServiceBusClient(authenticationSettings.ServiceBusConnectionStringOrFullyQualifiedName, creds, options);
        }
        else
        {
            client = new ServiceBusClient(authenticationSettings.ServiceBusConnectionStringOrFullyQualifiedName, options);
        }
        return client;
    }
}
