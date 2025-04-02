using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;

namespace RevolutionaryStuff.ApiCore.Services.HostedServices;

public sealed class PeriodicBackgroundService<TRunner>(
    IOptions<PeriodicBackgroundServiceConfig> _configOptions,
    BaseBackgroundService.BaseBackgroundServiceConstructorArgs _baseConstructorArgs,
    ILogger<PeriodicBackgroundService<TRunner>> _logger)
    : BaseBackgroundService(_baseConstructorArgs, _logger)
    where TRunner : IPeriodicServiceRunner
{
    private async Task GoAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        var scopedServiceProvider = scope.ServiceProvider;
        var runner = scopedServiceProvider.GetRequiredService<TRunner>();
        await runner.ExecuteAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var runNumber = 0; ; runNumber++)
        {
            var config = _configOptions.Value;
            var s = CrontabSchedule.Parse(config.CronSchedule,
                                  new CrontabSchedule.ParseOptions()
                                  {
                                      IncludingSeconds = config.IncludingSeconds
                                  });
            var now = DateTime.UtcNow;
            var next = s.GetNextOccurrence(DateTime.UtcNow);
            var delay = next.Subtract(now);
            var t = Task.Delay(delay, stoppingToken);
            LogWarning($"CometScheduledHostedService<{typeof(TRunner).Name}> will perform run#{runNumber} in {delay} seconds at {next.ToIsoString()}");
            await t;
            if (t.IsCanceled)
                return;

            try
            {
                await GoAsync();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }
    }
}

