using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Data.JsonStore.ChangeDataCapture;
using RevolutionaryStuff.Data.JsonStore.Entities;
using RevolutionaryStuff.Data.JsonStore.Repos;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore;

public static class Use
{
    public class Settings
    {
        public IJsonSerializer? JsonSerializer { get; set; }
        public IJsonEntityIdServices? JsonEntityIdServices { get; set; }
    }

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffDataJsonStore(Settings? settings = null)
                => sources.Use(
                    settings,
                    () =>
                    {
                        sources.UseRevolutionaryStuffCore();

                        if (settings?.JsonSerializer != null)
                        {
                            JsonSerializable.Serializer = settings.JsonSerializer;
                        }
                        if (settings?.JsonEntityIdServices != null)
                        {
                            JsonEntity.JsonEntityIdServices = settings.JsonEntityIdServices;
                        }

                        sources.AddScoped<JsonEntityRepoConstructorArgs>();
                        sources.ConfigureOptions<JsonEntityRepoBaseConfig>(JsonEntityRepoBaseConfig.ConfigSectionName);

                        sources.ConfigureOptions<JsonEntityContainerResolverConfig>(JsonEntityContainerResolverConfig.ConfigSectionName);
                        sources.AddSingleton<IJsonEntityContainerResolver, ConfigJsonEntityContainerResolver>();
                        sources.AddSingleton<IJsonEntityContainerResolver, AttributeJsonEntityContainerResolver>();
                    });
        public IServiceCollection AddScopedChangeDataCaptureJsonEntityController<TChangeDataCaptureJsonEntityController>()
                    where TChangeDataCaptureJsonEntityController : class, IChangeDataCaptureJsonEntityController
        {
            sources.AddScoped<TChangeDataCaptureJsonEntityController>();
            sources.AddScoped<IChangeDataCaptureJsonEntityController, TChangeDataCaptureJsonEntityController>();
            return sources;
        }
    }


}
