using System.Threading;

namespace RevolutionaryStuff.Core;

public static class SynchronizationHelpers
{
    extension(SemaphoreSlim source)
    {
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> funcAsync)
        {
            ArgumentNullException.ThrowIfNull(funcAsync);

            await source.WaitAsync();
            try
            {
                return await funcAsync();
            }
            finally
            {
                source.Release();
            }
        }
        public async Task ExecuteAsync(Func<Task> actionAsync)
        {
            ArgumentNullException.ThrowIfNull(actionAsync);

            await source.WaitAsync();
            try
            {
                await actionAsync();
            }
            finally
            {
                source.Release();
            }
        }
        public async Task<T> ExecuteAsync<T>(Func<T> func)
        {
            ArgumentNullException.ThrowIfNull(func);

            await source.WaitAsync();
            try
            {
                return func();
            }
            finally
            {
                source.Release();
            }
        }
        public async Task ExecuteAsync(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            await source.WaitAsync();
            try
            {
                action();
            }
            finally
            {
                source.Release();
            }
        }
    }




}
