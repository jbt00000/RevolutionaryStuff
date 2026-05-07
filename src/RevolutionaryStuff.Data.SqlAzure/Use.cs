using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Dapr;

public static class Use
{
    public sealed record Settings(RevolutionaryStuff.Azure.Use.Settings? RevolutionaryStuffAzureUseSettings);

    public static IServiceCollection UseRevolutionaryStuffDataSqlAzure(this IServiceCollection services, Settings? settings = null)
        => services.Use(
            settings,
            () =>
            {
                services.UseRevolutionaryStuffAzure(settings?.RevolutionaryStuffAzureUseSettings);

            });
}
