using Microsoft.Extensions.DependencyInjection;
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

    public static void UseRevolutionaryStuffAspNetCore(this IServiceCollection services, Settings settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffCore();
        services.AddScoped<ICorrelationIdFinder, HttpContextCorrelationIdFinder>();
        services.AddSingleton<IWebSessionArchiver, SazWebSessionArchiver>();
        services.AddSingleton<ISazWebSessionArchiver, SazWebSessionArchiver>();
    });
}
