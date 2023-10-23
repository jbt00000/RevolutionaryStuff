using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.AspNetCore.Services.Correlation;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.Correlation;

namespace RevolutionaryStuff.AspNetCore;

#pragma warning disable IDE1006 // Naming Styles
public static class _Use
#pragma warning restore IDE1006 // Naming Styles
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
