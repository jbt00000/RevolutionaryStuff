using System.Diagnostics;
using System.Threading;

namespace RevolutionaryStuff.Core.Threading;

public class PeriodicAction : BaseDisposable
{
    private readonly Timer T;

    public PeriodicAction(Func<Task<bool>> func, TimeSpan waitBetweenInvocationsDuration, TimeSpan? waitBeforeStartup = null)
        : this(() => { var res = func().ExecuteSynchronously(); return res; }, waitBetweenInvocationsDuration, waitBeforeStartup)
    { }

    public PeriodicAction(Func<Task> action, TimeSpan waitBetweenInvocationsDuration, TimeSpan? waitBeforeStartup = null)
        : this(() => action().ExecuteSynchronously(), waitBetweenInvocationsDuration, waitBeforeStartup)
    { }

    public PeriodicAction(Action action, TimeSpan waitBetweenInvocationsDuration, TimeSpan? waitBeforeStartup = null)
        : this(() => { action(); return false; }, waitBetweenInvocationsDuration, waitBeforeStartup)
    { }

    public PeriodicAction(Func<bool> action, TimeSpan waitBetweenInvocationsDuration, TimeSpan? waitBeforeStartup = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        bool cancel = false;

        T = new Timer(
            delegate (object state)
            {
                T.Change(-1, -1);
                if (IsDisposed) return;
                try
                {
                    cancel = action();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                finally
                {
                    if (!cancel)
                    {
                        T.Change(waitBetweenInvocationsDuration, waitBetweenInvocationsDuration);
                    }
                }
            },
            null,
            Timeout.InfiniteTimeSpan,
            Timeout.InfiniteTimeSpan);
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
