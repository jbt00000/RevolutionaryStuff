using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.AspNetCore.Services.Correlation;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.Correlation;

namespace RevolutionaryStuff.AspNetCore;

public static class _Use
{
    public class Settings
    {
    }

    public static void UseRevolutionaryStuffAspNetCore(this IServiceCollection services, Settings settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
    {
        services.AddScoped<ICorrelationIdFinder, HttpContextCorrelationIdFinder>();
    });
}
