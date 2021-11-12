namespace RevolutionaryStuff.Core.Threading;

public interface IDisposableReaderWriterLock : IDisposable
{
    IDisposable UseWrite();
}
