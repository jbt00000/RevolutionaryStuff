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

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffCrmMondayCom(Settings? settings = null)
                => sources.Use(
                    settings,
                    () =>
                    {
                        settings ??= new();
                        sources.UseRevolutionaryStuffCrm(settings.RevolutionaryStuffCrmSettings);
                        sources.ConfigureOptions<MondayComApiService.Config>(MondayComApiService.Config.ConfigSectionName);
                        sources.AddScoped<IMondayComApi, MondayComApiService>();
                        sources.ConfigureOptions<MondayComCrm.Config>(MondayComCrm.Config.ConfigSectionName);
                        sources.AddScoped<IMondayComCrm, MondayComCrm>();
                        if (settings.RegisterMondayComCrmAsICrm == true)
                        {
                            sources.AddScoped<ICrm, MondayComCrm>();
                        }
                    });
    }
}
