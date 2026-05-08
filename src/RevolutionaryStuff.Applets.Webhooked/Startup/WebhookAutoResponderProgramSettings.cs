using Microsoft.Extensions.DependencyInjection;

namespace RevolutionaryStuff.Applets.Webhooked;

public record WebhookAutoResponderProgramSettings
{
    public Action<IServiceCollection>? ConfigureServices { get; init; }
    public Use.Settings? WebhookedUseSettings { get; init; }
}
