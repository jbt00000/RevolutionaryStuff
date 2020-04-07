using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    public abstract class SimpleHostedService : BaseDisposable, IHostedService
    {
        private readonly ManualResetEvent ShutdownRequestedEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent ShutdownCompletedEvent = new ManualResetEvent(false);
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
}
