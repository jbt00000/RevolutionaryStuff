using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos;
#pragma warning disable IDE1006 // Naming Styles
public static class _Use
#pragma warning restore IDE1006 // Naming Styles
{
    public class Settings
    {
    }

    private static int InitCalls;
    public static void UseRevolutionaryStuffDataJsonStoreCosmos(this IServiceCollection services, Settings? settings = null)
    {
        if (Interlocked.Increment(ref InitCalls) > 1) return;

        #region Database

        services.ConfigureOptions<CosmosJsonEntityServer.Config>(CosmosJsonEntityServer.Config.ConfigSectionName);
        services.AddScoped<CosmosJsonEntityServer.CosmosJsonEntityServerConstructorArgs>();
        services.AddScoped<IJsonEntityServer, CosmosJsonEntityServer>();

        #endregion

    }
}
