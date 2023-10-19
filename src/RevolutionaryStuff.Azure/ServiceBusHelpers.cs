using Azure.Identity;
using Azure.Messaging.ServiceBus;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Azure;
public class ServiceBusHelpers
{
    public class ServiceBusClientAuthenticationSettings : IValidate
    {
        public string ServiceBusConnectionStringOrFullyQualifiedName { get; }
        public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;

        public void Validate()
            => ExceptionHelpers.AggregateExceptionsAndReThrow(
                () => Requires.Text(ServiceBusConnectionStringOrFullyQualifiedName),
                () => Requires.True(AuthenticateWithWithDefaultAzureCredentials)
                );

        public ServiceBusClientAuthenticationSettings() { }

        public ServiceBusClientAuthenticationSettings(string serviceBusConnectionStringOrFullyQualifiedName, bool authenticateWithWithDefaultAzureCredentials = true)
        {
            Requires.Text(serviceBusConnectionStringOrFullyQualifiedName);

            ServiceBusConnectionStringOrFullyQualifiedName = serviceBusConnectionStringOrFullyQualifiedName;
            AuthenticateWithWithDefaultAzureCredentials = authenticateWithWithDefaultAzureCredentials;
        }
    }

    public static ServiceBusClient ConstructServiceBusClient(ServiceBusClientAuthenticationSettings authenticationSettings)
    {
        Requires.Valid(authenticationSettings);

        if (authenticationSettings.AuthenticateWithWithDefaultAzureCredentials)
        {
            var creds = new DefaultAzureCredential(new DefaultAzureCredentialOptions());
            return new ServiceBusClient(authenticationSettings.ServiceBusConnectionStringOrFullyQualifiedName, creds);
        }
        else
        {
            return new ServiceBusClient(authenticationSettings.ServiceBusConnectionStringOrFullyQualifiedName);
        }
    }
}
