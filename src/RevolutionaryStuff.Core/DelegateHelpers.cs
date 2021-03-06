﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core
{
    public static class DelegateHelpers
    {
        private static bool AllowAllExceptions(Exception ex) { return true; }

        public static async Task<T> CallAndRetryOnFailureAsync<T>(this Func<Task<T>> func, int? retryCount = 5, TimeSpan? backoffPeriod = null, Predicate<Exception> exceptionChecker = null)
        {
            exceptionChecker = exceptionChecker ?? AllowAllExceptions;
            backoffPeriod = backoffPeriod ?? TimeSpan.FromMilliseconds(250);
            int z = 0;
            for (;;)
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    if (!exceptionChecker(ex)) throw;
                    if (z++ < retryCount.GetValueOrDefault(3))
                    {
                        var wait = Convert.ToInt32(backoffPeriod.Value.TotalMilliseconds * z);
                        Trace.WriteLine(string.Format("CallAndRetryOnFailure retry={0} wait={1}", z, TimeSpan.FromMilliseconds(wait)));
                        await (Task.Delay(wait));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public static T CallAndRetryOnFailure<T>(this Func<T> func, int? retryCount = 3, TimeSpan? backoffPeriod = null, Predicate<Exception> exceptionChecker = null)
        {
            exceptionChecker = exceptionChecker ?? AllowAllExceptions;
            backoffPeriod = backoffPeriod ?? TimeSpan.FromSeconds(2);
            int z = 0;
            for (;;)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    if (!exceptionChecker(ex)) throw;
                    if (z++ < retryCount.GetValueOrDefault(3))
                    {
                        var wait = Convert.ToInt32(backoffPeriod.Value.TotalMilliseconds * z);
                        Trace.WriteLine(string.Format("CallAndRetryOnFailure retry={0} wait={1}", z, TimeSpan.FromMilliseconds(wait)));
                        System.Threading.Thread.Sleep(wait);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public static T CallAndRetryOnFailure<T, E>(this Func<T> func, int? retryCount = 3, TimeSpan? backoffPeriod = null) where E : Exception
        {
            Predicate<Exception> exceptionChecker = delegate (Exception ex)
            {
                return ex is E;
            };
            return CallAndRetryOnFailure(func, retryCount, backoffPeriod, exceptionChecker);
        }

        /// <summary>
        /// Enter a critical section and act, or if someone is already inside, wait, but don't execute
        /// </summary>
        /// <param name="actor">The action to take</param>
        /// <param name="locker">A lock</param>
        public static void SingleActor(this Action actor, object locker = null)
        {
            Requires.NonNull(actor, nameof(actor));
            locker = locker ?? actor;
            if (Monitor.TryEnter(locker))
            {
                try
                {
                    actor();
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

        public static void SafeInvoke<EA>(this EventHandler<EventArgs<EA>> h, object sender, EventArgs<EA> ea, bool throwException = false)
        {
            try
            {
                h?.Invoke(sender, ea);
            }
            catch (Exception ex)
            {
                if (throwException)
                {
                    throw ex;
                }
                else
                {
#if DEBUG
                    Debug.WriteLine(ex);
#endif
                }
            }
        }

        public static void SafeInvoke(this EventHandler h, object sender, EventArgs ea = null, bool throwException = false)
        {
            try
            {
                h?.Invoke(sender, ea ?? EventArgs.Empty);
            }
            catch (Exception ex)
            {
                if (throwException)
                {
                    throw ex;
                }
                else
                {
#if DEBUG
                    Debug.WriteLine(ex);
#endif
                }
            }
        }

        public static bool SafeInvoke(this CancelEventHandler h, object sender, CancelEventArgs ea = null, bool throwException = false)
        {
            ea = ea ?? new CancelEventArgs();
            try
            {
                h?.Invoke(sender, ea);
            }
            catch (Exception ex)
            {
                if (throwException)
                {
                    throw ex;
                }
                else
                {
#if DEBUG
                    Debug.WriteLine(ex);
#endif
                }
            }
            return ea.Cancel;
        }
    }
}
