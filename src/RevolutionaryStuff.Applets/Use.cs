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

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffApplets(Settings? settings = null)
                => sources.Use<Settings>(
                    settings,
                    () =>
            {
                sources.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreUseSettings);

                #region TextTemplateRenderers
                sources.AddTextTemplateRenderer<IMustacheTextTemplateRenderer, MustacheTextTemplateRenderer>(ServiceLifetime.Singleton);
                sources.AddTextTemplateRenderer<IScribanTextTemplateRenderer, ScribanTextTemplateRenderer>(ServiceLifetime.Singleton);
                #endregion

            });
    }
}
