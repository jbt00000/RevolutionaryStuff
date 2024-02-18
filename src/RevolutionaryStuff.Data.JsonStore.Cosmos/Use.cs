using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos;

public static class Use
{
    public class Settings
    {
    }

    public static void UseRevolutionaryStuffDataJsonStoreCosmos(this IServiceCollection services, Settings? settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
            {
                #region Database

                services.ConfigureOptions<CosmosJsonEntityServerConfig>(CosmosJsonEntityServerConfig.ConfigSectionName);
                services.AddScoped<CosmosJsonEntityServerConstructorArgs>();

                #endregion
            });
}
