using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Azure;

namespace RevolutionaryStuff.Dapr;

public static class Use
{
    public record Settings(RevolutionaryStuff.Azure.Use.Settings? RevolutionaryStuffAzureUseSettings)
    { }

    public static void UseRevolutionaryStuffDataSqlAzure(this IServiceCollection services, Settings? settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
            {
                services.UseRevolutionaryStuffAzure(settings?.RevolutionaryStuffAzureUseSettings);

            });
}
