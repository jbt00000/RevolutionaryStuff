using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure.Services.Authentication;
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
        public RevolutionaryStuff.Core.Use.Settings RevolutionaryStuffCoreUseSettings { get; set; }
    }

    public static void UseRevolutionaryStuffAzure(this IServiceCollection services, Settings settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreUseSettings);

        services.AddSingleton<BaseWorker.BaseWorkerConstructorArgs>();

        services.ConfigureOptions<DefaultAzureTokenCredentialProvider.Config>(DefaultAzureTokenCredentialProvider.Config.ConfigSectionName);
        services.AddSingleton<IAzureTokenCredentialProvider, DefaultAzureTokenCredentialProvider>();

        services.ConfigureOptions<ServiceBusWorker.Config>(settings?.ServiceBusWorkerConfigSectionName ?? ServiceBusWorker.Config.ConfigSectionName);
        services.ConfigureOptions<ServiceBusMessageSender.ServiceBusMessageSenderConfig>(settings?.ServiceBusMessageSenderConfigSectionName ?? ServiceBusMessageSender.ServiceBusMessageSenderConfig.ConfigSectionName);
        services.AddScoped<ServiceBusMessageSender.ServiceBusMessageSenderConstructorArgs>();
        services.AddScoped<IServiceBusMessageSender, DefaultServiceBusMessageSender>();
    });
}
