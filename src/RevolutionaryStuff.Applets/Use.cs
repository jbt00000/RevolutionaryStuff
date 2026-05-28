using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Applets.Services.TextTemplateRenderers;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Applets;

public static class Use
{
    public class Settings
    {
        public RevolutionaryStuff.Core.Use.Settings? RevolutionaryStuffCoreUseSettings { get; set; }
    }

    public static IServiceCollection UseRevolutionaryStuffApplets(this IServiceCollection services, Settings? settings = null)
        => services.Use<Settings>(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreUseSettings);

        #region TextTemplateRenderers
        services.AddTextTemplateRenderer<IMustacheTextTemplateRenderer, MustacheTextTemplateRenderer>(ServiceLifetime.Singleton);
        services.AddTextTemplateRenderer<IScribanTextTemplateRenderer, ScribanTextTemplateRenderer>(ServiceLifetime.Singleton);
        #endregion

    });
}
