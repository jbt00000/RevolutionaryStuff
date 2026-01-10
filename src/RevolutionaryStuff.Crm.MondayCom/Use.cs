using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Crm.MondayCom.Implementation;

namespace RevolutionaryStuff.Crm.MondayCom;

public static class Use
{
    public class Settings
    {
        public bool RegisterMondayComCrmAsICrm { get; set; } = true;
        public RevolutionaryStuff.Crm.Use.Settings? RevolutionaryStuffCrmSettings { get; set; }
    }

    public static void UseRevolutionaryStuffCrmMondayCom(this IServiceCollection services, Settings? settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
            {
                settings ??= new();
                services.UseRevolutionaryStuffCrm(settings.RevolutionaryStuffCrmSettings);
                services.ConfigureOptions<MondayComApiService.Config>(MondayComApiService.Config.ConfigSectionName);
                services.AddScoped<IMondayComApi, MondayComApiService>();
                services.ConfigureOptions<MondayComCrm.Config>(MondayComCrm.Config.ConfigSectionName);
                services.AddScoped<IMondayComCrm, MondayComCrm>();
                if (settings.RegisterMondayComCrmAsICrm == true)
                {
                    services.AddScoped<ICrm, MondayComCrm>();
                }
            });
}
