using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCrontab;

namespace RevolutionaryStuff.Applets.Services.Runners;

public static class ScheduledRunnerServiceCollectionExtensions
{
    extension(IServiceCollection sources)
    {
        public IServiceCollection AddScheduledRunnerHost<TRunner>(
                IConfiguration configuration,
                string configSectionName)
                where TRunner : class, IRunner
        {
            ArgumentNullException.ThrowIfNull(sources);
            ArgumentNullException.ThrowIfNull(configuration);
            Requires.Text(configSectionName);

            sources.AddScoped<TRunner>();
            sources.AddOptions<ScheduledRunnerHostConfig>(GetOptionsName<TRunner>())
                .Bind(configuration.GetSection(configSectionName))
                .ValidateDataAnnotations()
                .Validate(
                    config => TryParseSchedule(config.Schedule),
                    $"The '{configSectionName}' schedule must be a valid five-part cron expression.")
                .ValidateOnStart();
            sources.AddHostedService<ScheduledRunnerHost<TRunner>>();

            return sources;
        }
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
