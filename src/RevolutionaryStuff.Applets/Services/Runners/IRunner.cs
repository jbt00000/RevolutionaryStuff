using System.Threading;

namespace RevolutionaryStuff.Applets.Services.Runners;

public interface IRunner
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
