using System.Threading;
using Microsoft.Extensions.Hosting;

namespace RevolutionaryStuff.Core.ApplicationParts;

public abstract class SimpleHostedService : BaseDisposable, IHostedService
{
    private readonly ManualResetEvent ShutdownRequestedEvent = new(false);
    private readonly ManualResetEvent ShutdownCompletedEvent = new(false);
    protected WaitHandle ShutdownRequested => ShutdownRequestedEvent;

    protected abstract Task OnStartAsync();

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await OnStartAsync();
        }
        finally
        {
            ShutdownCompletedEvent.Set();
        }
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        ShutdownRequestedEvent.Set();
        ShutdownCompletedEvent.WaitOne();
        return Task.CompletedTask;
    }
}
