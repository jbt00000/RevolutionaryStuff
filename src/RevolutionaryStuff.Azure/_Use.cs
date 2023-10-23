using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Azure;

#pragma warning disable IDE1006 // Naming Styles
public static class _Use
#pragma warning restore IDE1006 // Naming Styles
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
        services.AddScoped<IServiceBusMessageSender, DefaultServiceBusMessageSender>();
    });
}
