using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace RevolutionaryStuff.Core;

public static class DelegateHelpers
{
    private static bool AllowAllExceptions(Exception ex) { return true; }

    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);
    extension<TEventArgs>(AsyncEventHandler<TEventArgs> source)
    {
        public Task NullSafeInvokeAsync(object sender, TEventArgs e)
        {
            if (source == null) return Task.CompletedTask;
            var handlers = source.GetInvocationList().Cast<AsyncEventHandler<TEventArgs>>().ToList();
            var tasks = handlers.Select(z => z(sender, e)).ToList();
            return Task.WhenAll(tasks);
        }
    }

    extension<T>(Func<Task<T>> source)
    {
        public async Task<T> CallAndRetryOnFailureAsync(int? retryCount = 5, TimeSpan? backoffPeriod = null, Predicate<Exception> exceptionChecker = null)
        {
            exceptionChecker ??= AllowAllExceptions;
            backoffPeriod ??= TimeSpan.FromMilliseconds(250);
            var z = 0;
            for (; ; )
            {
                try
                {
                    return await source();
                }
                catch (Exception ex)
                {
                    if (!exceptionChecker(ex)) throw;
                    if (z++ < retryCount.GetValueOrDefault(3))
                    {
                        var wait = Convert.ToInt32(backoffPeriod.Value.TotalMilliseconds * z);
                        Trace.WriteLine($"CallAndRetryOnFailure retry={z} wait={TimeSpan.FromMilliseconds(wait)}");
                        await Task.Delay(wait);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }

    extension<T>(Func<T> source)
    {
        public T CallAndRetryOnFailure(int? retryCount = 3, TimeSpan? backoffPeriod = null, Predicate<Exception> exceptionChecker = null)
        {
            exceptionChecker ??= AllowAllExceptions;
            backoffPeriod ??= TimeSpan.FromSeconds(2);
            var z = 0;
            for (; ; )
            {
                try
                {
                    return source();
                }
                catch (Exception ex)
                {
                    if (!exceptionChecker(ex)) throw;
                    if (z++ < retryCount.GetValueOrDefault(3))
                    {
                        var wait = Convert.ToInt32(backoffPeriod.Value.TotalMilliseconds * z);
                        Trace.WriteLine($"CallAndRetryOnFailure retry={z} wait={TimeSpan.FromMilliseconds(wait)}");
                        Thread.Sleep(wait);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        public T CallAndRetryOnFailure<E>(int? retryCount = 3, TimeSpan? backoffPeriod = null) where E : Exception
        {
            static bool exceptionChecker(Exception ex) => ex is E;
            return CallAndRetryOnFailure(source, retryCount, backoffPeriod, exceptionChecker);
        }
    }



    extension(Action source)
    {
        /// <summary>
        /// Enter a critical section and act, or if someone is already inside, wait, but don't execute
        /// </summary>
        /// <param name="source">The action to take</param>
        /// <param name="locker">A lock</param>
        public void SingleActor(object locker = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            locker ??= source;
            if (Monitor.TryEnter(locker))
            {
                try
                {
                    source();
                }
                finally
                {
                    Monitor.Exit(locker);
                }
            }
            else
            {
                lock (locker) { }
            }
        }
    }

    extension<EA>(EventHandler<EventArgs<EA>> source)
    {
        public void SafeInvoke(object sender, EventArgs<EA> ea, bool throwException = false)
        {
            try
            {
                source?.Invoke(sender, ea);
            }
            catch (Exception ex)
            {
                if (throwException)
                {
                    throw;
                }
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
        }
    }

    extension(EventHandler source)
    {
        public void SafeInvoke(object sender, EventArgs ea = null, bool throwException = false)
        {
            try
            {
                source?.Invoke(sender, ea ?? EventArgs.Empty);
            }
            catch (Exception ex)
            {
                if (throwException)
                {
                    throw;
                }
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
        }
    }

    extension(CancelEventHandler source)
    {
        public bool SafeInvoke(object sender, CancelEventArgs ea = null, bool throwException = false)
        {
            ea ??= new CancelEventArgs();
            try
            {
                source?.Invoke(sender, ea);
            }
            catch (Exception ex)
            {
                if (throwException)
                {
                    throw;
                }
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
            return ea.Cancel;
        }
    }
}
