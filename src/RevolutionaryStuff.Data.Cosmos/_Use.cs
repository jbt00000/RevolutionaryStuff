using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.Cosmos;

#pragma warning disable IDE1006 // Naming Styles
public static class _Use
#pragma warning restore IDE1006 // Naming Styles
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
