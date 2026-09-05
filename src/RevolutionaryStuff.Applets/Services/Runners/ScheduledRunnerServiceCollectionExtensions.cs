using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCrontab;

namespace RevolutionaryStuff.Applets.Services.Runners;

public static class ScheduledRunnerServiceCollectionExtensions
{
    public static IServiceCollection AddScheduledRunnerHost<TRunner>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName)
        where TRunner : class, IRunner
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        Requires.Text(configSectionName);

        services.AddScoped<TRunner>();
        services.AddOptions<ScheduledRunnerHostConfig>(GetOptionsName<TRunner>())
            .Bind(configuration.GetSection(configSectionName))
            .ValidateDataAnnotations()
            .Validate(
                config => TryParseSchedule(config.Schedule),
                $"The '{configSectionName}' schedule must be a valid five-part cron expression.")
            .ValidateOnStart();
        services.AddHostedService<ScheduledRunnerHost<TRunner>>();

        return services;
    }

    internal static string GetOptionsName<TRunner>()
        => typeof(TRunner).FullName ?? typeof(TRunner).Name;

    private static bool TryParseSchedule(string schedule)
    {
        if (string.IsNullOrWhiteSpace(schedule)) return false;

        try
        {
            _ = CrontabSchedule.Parse(schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = false });
            return true;
        }
        catch (CrontabException)
        {
            return false;
        }
    }
}
