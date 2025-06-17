using System.Diagnostics;
using System.Threading;

namespace RevolutionaryStuff.Core;

/// <summary>
/// This class provides an abstract implementation of IDisposable
/// That makes it easier for subclasses to handle dispose
/// </summary>
public abstract class BaseDisposable : IDisposable
{
#if DEBUG //hmm... As this is a deubg method, may not be visible after being imported via nuget...
#pragma warning disable IDE0052 // Remove unread private members
    private static long ObjectId_s;
    private readonly long ObjectId_p = Interlocked.Increment(ref ObjectId_s);
#pragma warning restore IDE0052 // Remove unread private members
#endif
    #region Constructors

    ~BaseDisposable()
    {
        MyDispose(false);
    }

    #endregion

    private int IsDisposed_p;

    /// <summary>
    /// Returns true if dispose has been called
    /// </summary>
    protected bool IsDisposed
    {
        [DebuggerStepThrough]
        get { return IsDisposed_p > 0; }
    }

    protected void CheckNotDisposed()
    {
        if (IsDisposed) throw new ObjectDisposedException("This object was already disposed");
    }

    #region IDisposable Members

    public void Dispose()
    {
        MyDispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    private void MyDispose(bool disposing)
    {
        if (1 != Interlocked.Increment(ref IsDisposed_p)) return;
        OnDispose(disposing);
        DisposeActions.NullSafeEnumerable().ForEach(a => a());
    }

    private IList<Action> DisposeActions;

    protected void RegisterDisposeAction(Action a)
    {
        if (a == null) return;
        DisposeActions ??= [];
        DisposeActions.Add(a);
    }

    protected void RegisterDisposableObject(IDisposable d)
    {
        if (d == null) return;
        RegisterDisposeAction(() => Stuff.Dispose(d));
    }

    /// <summary>
    /// Override this function to handle calls to dispose.
    /// This will only get called once
    /// </summary>
    /// <param name="disposing">True when the object is being disposed, 
    /// false if it is being destructed</param>
    protected virtual void OnDispose(bool disposing)
    {
    }
}
