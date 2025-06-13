using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.JsonStore.Cosmos.Repos;
using RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;
using static RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer.DefaultCosmosJsonEntityServer;

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

                services.AddScoped<DefaultCosmosJsonEntityServerConstructorArgs>();
                services.AddScoped<CosmosRepoConstructorArgs>();
            });
}
