using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Crm;
public static class Use
{
    public class Settings
    {
        public RevolutionaryStuff.Core.Use.Settings? RevolutionaryStuffCoreSettings { get; set; }
    }

    public static void UseRevolutionaryStuffCrm(this IServiceCollection services, Settings? settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
            {
                services.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreSettings);
            });
}
