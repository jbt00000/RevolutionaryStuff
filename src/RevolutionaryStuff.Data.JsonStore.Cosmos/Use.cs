using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.JsonStore.Cosmos.ChangeDataCapture;
using RevolutionaryStuff.Data.JsonStore.Cosmos.Repos;
using RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos;

public static class Use
{
    public class Settings
    {
    }

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffDataJsonStoreCosmos(Settings? settings = null)
                => sources.Use(
                    settings,
                    () =>
                    {
                        sources.UseRevolutionaryStuffDataJsonStore();

                        #region Database

                        sources.ConfigureOptions<CosmosJsonEntityServerConfig>(CosmosJsonEntityServerConfig.ConfigSectionName);
                        sources.AddScoped<CosmosJsonEntityServerConstructorArgs>();

                        #endregion

                        sources.AddScoped<CosmosChangeDataCaptureJsonEntityEventProcessor>();
                        sources.AddScoped<CosmosChangeDataCaptureJsonEntityEventProcessor.CosmosChangeDataCaptureJsonEntityEventProcessorConstructorArgs>();
                        sources.AddScoped<DefaultCosmosJsonEntityServer.DefaultCosmosJsonEntityServerConstructorArgs>();
                        sources.AddScoped<CosmosRepoConstructorArgs>();
                    });
    }
}
