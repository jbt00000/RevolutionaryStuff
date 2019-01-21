using System;

namespace RevolutionaryStuff.Core.Threading
{
    public interface IDisposableReaderWriterLock
    {
        IDisposable UseWrite();
    }
}