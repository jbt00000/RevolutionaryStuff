using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.Cosmos.BackgroundServices;
using RevolutionaryStuff.Data.Cosmos.Services.Tools;

namespace RevolutionaryStuff.Data.Cosmos;

public static class Use
{
    public class Settings
    {
    }

    public static IServiceCollection UseRevolutionaryStuffDataCosmos(this IServiceCollection services, Settings? settings = null)
        => services.Use(
            settings,
            () =>
            {
                services.UseRevolutionaryStuffAzure();
                services.ConfigureOptions<CosmosChangeFeedBackgroundServiceConfig>(CosmosChangeFeedBackgroundServiceConfig.ConfigSectionName);
                services.AddTransient<ICosmosFieldCopier, CosmosFieldCopier>();
            });
}
