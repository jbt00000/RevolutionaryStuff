using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.Threading;

/// <remarks>https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/</remarks>
public sealed class AsyncLocker : BaseDisposable
{
    private readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

    public AsyncLocker()
    { }

    public async Task GoAsync(Func<Task> a)
    {
        await Semaphore.WaitAsync();
        try
        {
            await a();
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
