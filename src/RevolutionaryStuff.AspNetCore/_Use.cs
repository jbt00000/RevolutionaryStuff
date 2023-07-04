using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;

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
    });
}
