using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Maui;

public static class Use
{
    public class Settings
    {
        public RevolutionaryStuff.Core.Use.Settings RevolutionaryStuffCoreUseSettings { get; set; }
    }

    public static IServiceCollection UseRevolutionaryStuffMaui(this IServiceCollection services, Settings? settings = null)
        => services.Use(
            settings,
            () =>
            {
                services.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreUseSettings);
            });
}
