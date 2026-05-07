using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Applets;

public static class Use
{
    public class Settings
    {
        public RevolutionaryStuff.Core.Use.Settings? RevolutionaryStuffCoreUseSettings { get; set; }
    }

    public static IServiceCollection UseRevolutionaryStuffApplets(this IServiceCollection services, Settings? settings = null)
        => services.Use(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreUseSettings);
    });
}


