using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.Cosmos.Workers;

namespace RevolutionaryStuff.Data.Cosmos;

public static class Use
{
    public class Settings
    {
    }

    public static void UseRevolutionaryStuffDataCosmos(this IServiceCollection services, Settings? settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
            {
                services.UseRevolutionaryStuffAzure();
                services.ConfigureOptions<CosmosChangeFeedProcessorWorker.Config>(CosmosChangeFeedProcessorWorker.Config.ConfigSectionName);
            });
}
