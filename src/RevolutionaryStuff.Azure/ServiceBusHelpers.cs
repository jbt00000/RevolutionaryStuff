using Azure.Identity;
using Azure.Messaging.ServiceBus;

namespace RevolutionaryStuff.Azure;
public class ServiceBusHelpers
{
    public static ServiceBusClient ConstructServiceBusClient(string connectionString, bool authenticateWithWithDefaultAzureCredentials)
    {
        if (authenticateWithWithDefaultAzureCredentials)
        {
            var creds = new DefaultAzureCredential(new DefaultAzureCredentialOptions());
            return new ServiceBusClient(connectionString, creds);
        }
        else
        {
            return new ServiceBusClient(connectionString);
        }
    }
}
