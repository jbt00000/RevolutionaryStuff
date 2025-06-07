namespace RevolutionaryStuff.Storage.Implementation.Base;

public abstract class BaseEntry<SP> : BaseLoggingDisposable, IEntry
    where SP : BaseStorageProvider
{
    public readonly SP StorageProvider;

    public BaseEntry(SP provider)
        : base(provider.StorageProviderLogger)
    {
        ArgumentNullException.ThrowIfNull(provider);

        StorageProvider = provider;
    }

    protected abstract string AbsolutePath { get; }

    IStorageProvider IEntry.StorageProvider
        => StorageProvider;

    public abstract DateTimeOffset LastModified { get; }

    public virtual string Name
    {
        get
        {
            var ret = StorageProvider.GetPathSegments(AbsolutePath).Last();
            LogTrace(".Name=>[{entryName}]", ret);
            return ret;
        }
    }

    public virtual string Path
    {
        get
        {
            var ret = StorageProvider.GetRootRelativePath(AbsolutePath);
            LogTrace(".Name=>[{entryPath}]", ret);
            return ret;
        }
    }

    public override string ToString()
        => $"{GetType().Name} {Path}";
}
