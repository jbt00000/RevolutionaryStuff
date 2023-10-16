using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos;
#pragma warning disable IDE1006 // Naming Styles
public static class _Use
#pragma warning restore IDE1006 // Naming Styles
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
