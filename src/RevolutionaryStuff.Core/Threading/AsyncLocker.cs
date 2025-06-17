using System.Threading;

namespace RevolutionaryStuff.Core.Threading;

/// <remarks>https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/</remarks>
public sealed class AsyncLocker : BaseDisposable
{
    private readonly SemaphoreSlim Semaphore = new(1, 1);

    public AsyncLocker()
    { }

    public async Task GoAsync(Func<Task> a)
    {
        CheckNotDisposed();
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

    public async Task<T> GoAsync<T>(Func<Task<T>> a)
    {
        CheckNotDisposed();
        await Semaphore.WaitAsync();
        try
        {
            return await a();
        }
        finally
        {
            Semaphore.Release();
        }
    }
    
    protected override void OnDispose(bool disposing)
    {
        if (disposing)
        {
            Semaphore.Dispose();
        }
        base.OnDispose(disposing);
    }
}
