using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus;
using RevolutionaryStuff.Azure.Workers;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Azure;

public static class Use
{
    public class Settings
    {
        public string ServiceBusWorkerConfigSectionName { get; set; }
        public string ServiceBusMessageSenderConfigSectionName { get; set; }
    }

    public static void UseRevolutionaryStuffAzure(this IServiceCollection services, Settings settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffCore();

        services.AddSingleton<BaseWorker.BaseWorkerConstructorArgs>();
        services.ConfigureOptions<ServiceBusWorker.Config>(settings?.ServiceBusWorkerConfigSectionName ?? ServiceBusWorker.Config.ConfigSectionName);
        services.ConfigureOptions<ServiceBusMessageSender.ServiceBusMessageSenderConfig>(settings?.ServiceBusMessageSenderConfigSectionName ?? ServiceBusMessageSender.ServiceBusMessageSenderConfig.ConfigSectionName);
        services.AddScoped<ServiceBusMessageSender.ServiceBusMessageSenderConstructorArgs>();
        services.AddScoped<IServiceBusMessageSender, DefaultServiceBusMessageSender>();
    });
}
