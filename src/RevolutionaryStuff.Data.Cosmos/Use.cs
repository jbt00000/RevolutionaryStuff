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

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffDataCosmos(Settings? settings = null)
                => sources.Use(
                    settings,
                    () =>
                    {
                        sources.UseRevolutionaryStuffAzure();
                        sources.ConfigureOptions<CosmosChangeFeedBackgroundServiceConfig>(CosmosChangeFeedBackgroundServiceConfig.ConfigSectionName);
                        sources.AddTransient<ICosmosFieldCopier, CosmosFieldCopier>();
                    });
    }
}
