using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure;
using RevolutionaryStuff.Core.ApplicationParts;

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
