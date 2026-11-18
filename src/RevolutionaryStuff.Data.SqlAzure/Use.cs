using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Dapr;

public static class Use
{
    public sealed record Settings(RevolutionaryStuff.Azure.Use.Settings? RevolutionaryStuffAzureUseSettings);

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffDataSqlAzure(Settings? settings = null)
                => sources.Use(
                    settings,
                    () =>
                    {
                        sources.UseRevolutionaryStuffAzure(settings?.RevolutionaryStuffAzureUseSettings);

                    });
    }
}
