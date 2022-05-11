namespace RevolutionaryStuff.Core.ApplicationParts;

public sealed class Flusher : IFlushable
{
    private readonly Action Flush;

    public Flusher(Action flush)
    {
        Requires.NonNull(flush);
        Flush = flush;
    }

    void IFlushable.Flush()
    {
        Flush();
    }
}
