using System;
using System.Threading;

namespace RevolutionaryStuff.Core.Threading
{
    public static class ReaderWriterLockHelpers
    {
        public static IDisposableReaderWriterLock UseRead(this ReaderWriterLock rwl)
            => new DisposableReaderWriterLock(rwl);

        public static IDisposable UseWrite(this ReaderWriterLock rwl)
            => new DisposableReaderWriterLock(rwl).UseWrite(true);
    }
}
