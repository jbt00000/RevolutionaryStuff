using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.AspNetCore.Services;
using RevolutionaryStuff.AspNetCore.Services.Correlation;
using RevolutionaryStuff.AspNetCore.Services.SazGenerators;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.Correlation;

namespace RevolutionaryStuff.AspNetCore;

public static class Use
{
    public class Settings
    {
    }

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffAspNetCore(Settings settings = null)
                => sources.Use(
                    settings,
                    () =>
            {
                sources.UseRevolutionaryStuffCore();
                sources.AddScoped<ICorrelationIdFinder, HttpContextCorrelationIdFinder>();
                sources.AddSingleton<IWebSessionArchiver, SazWebSessionArchiver>();
                sources.AddSingleton<ISazWebSessionArchiver, SazWebSessionArchiver>();
                sources.AddTextTemplateRenderer<IRazorTextTemplateRenderer, RazorTextTemplateRenderer>(ServiceLifetime.Singleton);
            });
    }
}
