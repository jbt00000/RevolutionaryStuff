using System;
using System.Diagnostics;
using System.Threading;

namespace RevolutionaryStuff.Core.Threading
{
    public class PeriodicAction : BaseDisposable
    {
        private readonly Timer T;

        public PeriodicAction(Action action, TimeSpan waitBetweenInvocationsDuration, TimeSpan? waitBeforeStartup = null)
        {
            Requires.NonNull(action, nameof(action));

            T = new Timer(
                delegate (object state)
                {
                    T.Change(-1, -1);
                    if (this.IsDisposed) return;
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                    finally
                    {
                        T.Change(waitBetweenInvocationsDuration, waitBetweenInvocationsDuration);
                    }
                },
                null,
                System.Threading.Timeout.InfiniteTimeSpan,
                System.Threading.Timeout.InfiniteTimeSpan);
            /*
             We First create and set the variable for the timer, then invoke it.
             This is because in practice, we've seen times where the delegate was invoked and tried to access T before T was actually set
             */
            T.Change(
                waitBeforeStartup.GetValueOrDefault(TimeSpan.Zero),
                waitBetweenInvocationsDuration);
        }

        protected override void OnDispose(bool disposing)
        {
            T.Change(-1, -1);
            Stuff.Dispose(T);
            base.OnDispose(disposing);
        }
    }
}
