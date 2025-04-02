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
    public record Settings(bool AddDaprClient = true, ApiCore.Use.Settings? RevolutionaryStuffApiCoreUseSettings = null)
    { }

    public static void UseRevolutionaryStuffDapr(this IServiceCollection services, Settings? settings = null)
        => ServiceUseManager.Use(
            settings,
            () =>
            {
                services.UseRevolutionaryStuffApiCore(settings?.RevolutionaryStuffApiCoreUseSettings);

                services.ConfigureOptions<DaprConfig>(DaprConfig.ConfigSectionName);
                if (settings.AddDaprClient)
                {
                    services.AddDaprClient();
                    //                    services.AddSingleton(new DaprClientBuilder().Build());
                }
                services.AddSingleton<IMyDaprStateStore>(sp => new MyDaprStateStore(sp.GetRequiredService<DaprClient>(), sp.GetRequiredService<IOptions<DaprConfig>>().Value.MyStateStoreName));
                services.AddSingleton<ISharedDaprStateStore>(sp => new SharedDaprStateStore(sp.GetRequiredService<DaprClient>(), sp.GetRequiredService<IOptions<DaprConfig>>().Value.SharedStateStoreName));
            });

    public static void AddDaprRefitRestApi<TIApi>(this IServiceCollection services, string name)
    where TIApi : class
    {
        services.AddSingleton(sp =>
        {
            return RestService.For<TIApi>(DaprClient.CreateInvokeHttpClient(name));
        });
    }
}
