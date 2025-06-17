using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Crm.MondayCom.Implementation;

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
                services.ConfigureOptions<MondayComApiService.Config>(MondayComApiService.Config.ConfigSectionName);
                services.AddScoped<IMondayComApi, Implementation.MondayComApiService>();
                services.ConfigureOptions<MondayComCrm.Config>(MondayComCrm.Config.ConfigSectionName);
                services.AddScoped<IMondayComCrm, Implementation.MondayComCrm>();
            });
}
