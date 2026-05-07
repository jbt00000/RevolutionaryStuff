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

    public static void UseRevolutionaryStuffApplets(this IServiceCollection services, Settings? settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
    {
        services.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreUseSettings);
    });
}


