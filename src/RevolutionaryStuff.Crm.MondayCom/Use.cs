using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Crm.MondayCom;
public static class Use
{
    public class Settings
    {
        public RevolutionaryStuff.Crm.Use.Settings? RevolutionaryStuffCrmSettings { get; set; }
    }

    public static void UseRevolutionaryStuffCrmMondayCom(this IServiceCollection services, Settings? settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
            {
                services.UseRevolutionaryStuffCrm(settings?.RevolutionaryStuffCrmSettings);
            });
}
