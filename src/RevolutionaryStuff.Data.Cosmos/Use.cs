using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;

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
            });
}
