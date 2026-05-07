using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Applets.Webhooked;

public static class Use
{
    public class Settings
    {
        public RevolutionaryStuff.Applets.Use.Settings? RevolutionaryStuffAppletsUseSettings { get; set; }
    }

    public static IServiceCollection UseRevolutionaryStuffApplets(this IServiceCollection services, Settings? settings = null)
        => services.Use(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffApplets(settings?.RevolutionaryStuffAppletsUseSettings);
    });
}
