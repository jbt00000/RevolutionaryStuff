using System.Threading;

namespace RevolutionaryStuff.Core.Threading;

internal sealed class DisposableReaderWriterLock : BaseDisposable, IDisposableReaderWriterLock
{
    private readonly ReaderWriterLock RWL;

    internal DisposableReaderWriterLock(ReaderWriterLock rwl)
    {
        Requires.NonNull(rwl);

        RWL = rwl;
        RWL.AcquireReaderLock(int.MaxValue);
    }

    protected override void OnDispose(bool disposing)
    {
        base.OnDispose(disposing);
        RWL.ReleaseReaderLock();
    }

    public IDisposable UseWrite()
        => UseWrite(false);

    internal IDisposable UseWrite(bool disposeParent)
        => new WriterLock(this, disposeParent);

    private class WriterLock : BaseDisposable
    {
        private readonly DisposableReaderWriterLock Parent;
        private LockCookie LockCookie;
        private readonly bool DisposeParent;

        public WriterLock(DisposableReaderWriterLock rl, bool disposeParent)
        {
            DisposeParent = disposeParent;
            Parent = rl;
            LockCookie = Parent.RWL.UpgradeToWriterLock(int.MaxValue);
        }

        protected override void OnDispose(bool disposing)
        {
            base.OnDispose(disposing);
            Parent.RWL.DowngradeFromWriterLock(ref LockCookie);
            if (DisposeParent)
            {
                Parent.Dispose();
            }
        }
    }
}
