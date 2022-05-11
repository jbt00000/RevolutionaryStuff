using System.Threading;

namespace RevolutionaryStuff.Core;

public static class SynchronizationHelpers
{
    public static async Task<T> ExecuteAsync<T>(this SemaphoreSlim semaphore, Func<Task<T>> funcAsync)
    {
        Requires.NonNull(funcAsync);

        await semaphore.WaitAsync();
        try
        {
            return await funcAsync();
        }
        finally
        {
            semaphore.Release();
        }
    }
    public static async Task ExecuteAsync(this SemaphoreSlim semaphore, Func<Task> actionAsync)
    {
        Requires.NonNull(actionAsync);

        await semaphore.WaitAsync();
        try
        {
            await actionAsync();
        }
        finally
        {
            semaphore.Release();
        }
    }

    public static async Task<T> ExecuteAsync<T>(this SemaphoreSlim semaphore, Func<T> func)
    {
        Requires.NonNull(func);

        await semaphore.WaitAsync();
        try
        {
            return func();
        }
        finally
        {
            semaphore.Release();
        }
    }
    public static async Task ExecuteAsync(this SemaphoreSlim semaphore, Action action)
    {
        Requires.NonNull(action);

        await semaphore.WaitAsync();
        try
        {
            action();
        }
        finally
        {
            semaphore.Release();
        }
    }
}
