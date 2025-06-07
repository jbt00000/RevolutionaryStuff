using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace RevolutionaryStuff.Storage.Providers.LocalFileSystem;

internal class FileSystemWatchChangeToken : IChangeToken
{
    private readonly IList<CallbackState> CallbackStates =
        [];

    private readonly PhysicalStorageProvider StorageProvider;
    private readonly FileSystemWatcher Watcher;

    private int ChangeCount;

    public FileSystemWatchChangeToken(
        PhysicalStorageProvider storageProvider, FileSystemWatcher watcher)
    {
        StorageProvider = storageProvider;
        Watcher = watcher;
        Watcher.Changed += Watcher_Changed;
        Watcher.Created += Watcher_Created;
        Watcher.Deleted += Watcher_Deleted;
        Watcher.Renamed += Watcher_Renamed;
    }

    bool IChangeToken.HasChanged => ChangeCount > 0;

    bool IChangeToken.ActiveChangeCallbacks =>
        throw new NotImplementedException();

    IDisposable IChangeToken.RegisterChangeCallback(
        Action<object> callback, object state)
    {
        lock (CallbackStates)
        {
            var cs = new CallbackState(this, callback, state);
            CallbackStates.Add(cs);
            return cs;
        }
    }

    private void Watcher_Renamed(object sender, RenamedEventArgs e) =>
        ChangeHappened();

    private void Watcher_Deleted(object sender, FileSystemEventArgs e) =>
        ChangeHappened();

    private void Watcher_Created(object sender, FileSystemEventArgs e) =>
        ChangeHappened();

    private void Watcher_Changed(object sender, FileSystemEventArgs e) =>
        ChangeHappened();

    private void ChangeHappened()
    {
        Interlocked.Increment(ref ChangeCount);
        lock (CallbackStates)
        {
            foreach (var cs in CallbackStates)
            {
                try
                {
                    cs.Invoke();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }
    }

    private class CallbackState : BaseDisposable
    {
        public readonly Action<object> Callback;
        public readonly FileSystemWatchChangeToken ChangeToken;
        public readonly object State;

        public CallbackState(FileSystemWatchChangeToken changeToken,
                             Action<object> callback, object state)
        {
            ChangeToken = changeToken;
            Callback = callback;
            State = state;
        }

        protected override void OnDispose(bool disposing)
        {
            base.OnDispose(disposing);
            lock (ChangeToken.CallbackStates)
            {
                ChangeToken.CallbackStates.Remove(this);
            }
        }

        public void Invoke()
            => Callback(State);
    }
}
