using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;
using RevolutionaryStuff.Core.Services.Tenant;

namespace RevolutionaryStuff.Applets.Services.Runners;

public sealed class ScheduledRunnerHost<TRunner>(
    IOptionsMonitor<ScheduledRunnerHostConfig> configOptions,
    ITenantIdEnumerator tenantIdEnumerator,
    IServiceProvider serviceProvider,
    ILogger<ScheduledRunnerHost<TRunner>> logger)
    : BackgroundService
    where TRunner : class, IRunner
{
    private ScheduledRunnerHostConfig Config
        => configOptions.Get(ScheduledRunnerServiceCollectionExtensions.GetOptionsName<TRunner>());

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var runNumber = 0; !stoppingToken.IsCancellationRequested; runNumber++)
        {
            var schedule = CrontabSchedule.Parse(
                Config.Schedule,
                new CrontabSchedule.ParseOptions { IncludingSeconds = false });
            var now = DateTime.UtcNow;
            var next = schedule.GetNextOccurrence(now);
            var delay = next - now;

            logger.LogInformation(
                "Scheduled runner {RunnerType} will perform run {RunNumber} at {NextRun} after {Delay}",
                typeof(TRunner).Name,
                runNumber,
                next,
                delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scheduled runner {RunnerType} failed during run {RunNumber}", typeof(TRunner).Name, runNumber);
            }
        }
    }

    private Task RunAsync(CancellationToken cancellationToken)
    {
        if (Config.AllTenants)
        {
            return tenantIdEnumerator.ForEachScopedTenantAsync(async args =>
            {
                var runner = args.ServiceProvider.GetRequiredService<TRunner>();
                await runner.RunAsync(cancellationToken);
            });
        }

        return RunInScopeAsync(cancellationToken);
    }

    private async Task RunInScopeAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var runner = scope.ServiceProvider.GetRequiredService<TRunner>();
        await runner.RunAsync(cancellationToken);
    }
}
