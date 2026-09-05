using System.ComponentModel.DataAnnotations;

namespace RevolutionaryStuff.Applets.Services.Runners;

public sealed class ScheduledRunnerHostConfig
{
    public bool AllTenants { get; set; } = true;

    [Required]
    public string Schedule { get; set; }
}
