using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Crm;

public static class Use
{
    public class Settings
    {
        public RevolutionaryStuff.Core.Use.Settings? RevolutionaryStuffCoreSettings { get; set; }
    }

    extension(IServiceCollection sources)
    {
        public IServiceCollection UseRevolutionaryStuffCrm(Settings? settings = null)
                => sources.Use(
                    settings,
                    () =>
                    {
                        sources.UseRevolutionaryStuffCore(settings?.RevolutionaryStuffCoreSettings);

                        sources.AddScoped<Implementation.Crm.CrmConstructorArgs>();
                    });
    }
}
