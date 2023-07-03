using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Data.JsonStore.Entities;
using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace RevolutionaryStuff.Data.JsonStore;
#pragma warning disable IDE1006 // Naming Styles
public static class _Use
#pragma warning restore IDE1006 // Naming Styles
{
    public class Settings
    {
        public IJsonSerializer? JsonSerializer { get; set; }
        public IJsonEntityIdServices? JsonEntityIdServices { get; set; }
    }

    private static int InitCalls;
    public static void UseRevolutionaryStuffDataJsonStore(this IServiceCollection services, Settings? settings = null)
    {
        if (Interlocked.Increment(ref InitCalls) > 1) return;

        services.UseRevolutionaryStuffCore();

        if (settings?.JsonSerializer != null)
        {

            JsonSerializable.Serializer = settings.JsonSerializer;
        }
        if (settings?.JsonEntityIdServices != null)
        {
            JsonEntity.JsonEntityIdServices = settings.JsonEntityIdServices;
        }
    }
}
