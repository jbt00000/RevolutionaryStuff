using Dapr.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using RevolutionaryStuff.ApiCore;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Dapr.Configs;
using RevolutionaryStuff.Dapr.Services.StateStore;

namespace RevolutionaryStuff.Dapr;

public static class Use
{
    public sealed record Settings(bool AddDaprClient = true, ApiCore.Use.Settings? RevolutionaryStuffApiCoreUseSettings = null);

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffDapr(Settings? settings = null)
                => sources.Use(
                    settings,
                    () =>
                    {
                        sources.UseRevolutionaryStuffApiCore(settings?.RevolutionaryStuffApiCoreUseSettings);

                        sources.ConfigureOptions<DaprConfig>(DaprConfig.ConfigSectionName);
                        if (settings.AddDaprClient)
                        {
                            sources.AddDaprClient();
                            //                    services.AddSingleton(new DaprClientBuilder().Build());
                        }
                        sources.AddSingleton<IMyDaprStateStore>(sp => new MyDaprStateStore(sp.GetRequiredService<DaprClient>(), sp.GetRequiredService<IOptions<DaprConfig>>().Value.MyStateStoreName));
                        sources.AddSingleton<ISharedDaprStateStore>(sp => new SharedDaprStateStore(sp.GetRequiredService<DaprClient>(), sp.GetRequiredService<IOptions<DaprConfig>>().Value.SharedStateStoreName));
                    });
        public IServiceCollection AddDaprRefitRestApi<TIApi>(string name)
            where TIApi : class
        {
            sources.AddSingleton(sp =>
            {
                return RestService.ForGenerated<TIApi>(DaprClient.CreateInvokeHttpClient(name));
            });
            return sources;
        }
    }


}
