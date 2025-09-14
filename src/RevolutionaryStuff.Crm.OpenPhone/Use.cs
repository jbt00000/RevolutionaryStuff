using Microsoft.Extensions.DependencyInjection;
using Refit;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Crm.OpenPhone.Apis.OpenPhone;

namespace RevolutionaryStuff.Crm.OpenPhone;
public static class Use
{
    public class Settings
    {
        public bool RegisterOpenPhoneCrmAsICrm { get; set; } = true;
        public Crm.Use.Settings? RevolutionaryStuffCrmSettings { get; set; }
        public string OpenPhoneApiConfigSectionName { get; set; } = "OpenPhoneApi";
    }

    public static void UseRevolutionaryStuffCrmOpenPhone(this IServiceCollection services, Settings? settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
            {
                settings ??= new();
                services.UseRevolutionaryStuffCrm(settings.RevolutionaryStuffCrmSettings);
                //                services.ConfigureOptions<MondayComApiService.Config>(MondayComApiService.Config.ConfigSectionName);
                //               services.AddScoped<IMondayComApi, MondayComApiService>();
                //              services.ConfigureOptions<MondayComCrm.Config>(MondayComCrm.Config.ConfigSectionName);
                //            services.AddScoped<IMondayComCrm, MondayComCrm>();
                if (settings.RegisterOpenPhoneCrmAsICrm == true)
                    Stuff.NoOp();//                    services.AddScoped<ICrm, MondayComCrm>();

                #region APIs
                services.AddRefitClient<IOpenPhonePublicAPI>().ConfigureHttpClientWithApiConfig(settings.OpenPhoneApiConfigSectionName);
                #endregion
            });
}
