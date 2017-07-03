namespace RevolutionaryStuff.Core.Caching
{
    public interface ICacheEntry
    {
        object Value { get; }
        bool IsExpired { get; }
    }
}
