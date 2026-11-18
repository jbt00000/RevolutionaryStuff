using System.Threading;

namespace RevolutionaryStuff.Core.Threading;

public static class ReaderWriterLockHelpers
{
    extension(ReaderWriterLock source)
    {
        public IDisposableReaderWriterLock UseRead()
                => new DisposableReaderWriterLock(source);
        public IDisposable UseWrite()
                => new DisposableReaderWriterLock(source).UseWrite(true);
    }


}
