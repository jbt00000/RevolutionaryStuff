namespace RevolutionaryStuff.Core.ApplicationParts;

public interface IPrimaryKey
{
    object Key { get; }
}

public interface IPrimaryKey<TKey> : IPrimaryKey
{
    new TKey Key { get; }
}
