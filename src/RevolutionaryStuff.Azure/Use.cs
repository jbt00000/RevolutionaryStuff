using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure.BackgroundServices;
using RevolutionaryStuff.Azure.Services.Authentication;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;
using RevolutionaryStuff.Azure.Services.Messaging.Outbound.ServiceBus;
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

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffAzure(Settings settings = null)
                => sources.Use(
                    settings,
                    () =>
            {
                sources.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreUseSettings);

                sources.AddSingleton<RevolutionaryStuffBackgroundService.RevolutionaryStuffBackgroundServiceConstructorArgs>();

                sources.ConfigureOptions<DefaultAzureTokenCredentialProvider.Config>(DefaultAzureTokenCredentialProvider.Config.ConfigSectionName);
                sources.AddSingleton<IAzureTokenCredentialProvider, DefaultAzureTokenCredentialProvider>();

                sources.ConfigureOptions<DefaultMessageExecutor.Config>(DefaultMessageExecutor.Config.ConfigSectionName);
                sources.AddScoped<IDefaultMessageExecutor, DefaultMessageExecutor>();

                sources.ConfigureOptions<ServiceBusBackgroundService.Config>(settings?.ServiceBusWorkerConfigSectionName ?? ServiceBusBackgroundService.Config.ConfigSectionName);
                sources.ConfigureOptions<ServiceBusMessageSender.ServiceBusMessageSenderConfig>(settings?.ServiceBusMessageSenderConfigSectionName ?? ServiceBusMessageSender.ServiceBusMessageSenderConfig.ConfigSectionName);
                sources.AddScoped<ServiceBusMessageSender.ServiceBusMessageSenderConstructorArgs>();
                sources.AddScoped<IServiceBusMessageSender, DefaultServiceBusMessageSender>();
            });
    }
}
