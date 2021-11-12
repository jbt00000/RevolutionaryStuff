namespace RevolutionaryStuff.Core.ApplicationParts;

public interface IAsyncGetter<T>
{
    Task<T> GetAsync();
}
