using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Azure;

public static class _Use
{
    public class Settings
    {
        public string ServiceBusMessageSenderConfigSectionName { get; set; }
    }

    public static void UseRevolutionaryStuffAzure(this IServiceCollection services, Settings settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffCore();

        services.ConfigureOptions<ServiceBusMessageSender.ServiceBusMessageSenderConfig>(settings?.ServiceBusMessageSenderConfigSectionName ?? ServiceBusMessageSender.ServiceBusMessageSenderConfig.ConfigSectionName);
        services.AddScoped<ServiceBusMessageSender.ServiceBusMessageSenderConstructorArgs>();
    });
}
