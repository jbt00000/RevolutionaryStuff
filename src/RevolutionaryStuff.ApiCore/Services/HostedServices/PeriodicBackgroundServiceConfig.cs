namespace RevolutionaryStuff.ApiCore.Services.HostedServices;

public class PeriodicBackgroundServiceConfig
{
    public string? CronSchedule { get; set; }
    public bool IncludingSeconds { get; set; }
}
