using System.IO;

namespace RevolutionaryStuff.Storage.Implementation.Base;

public abstract class BaseFileEntry<SP> : BaseEntry<SP>, IFileEntry
    where SP : BaseStorageProvider
{
    protected BaseFileEntry(SP storageProvider)
        : base(storageProvider)
    { }

    public abstract long Length { get; }

    string IFileEntry.Extension
        => System.IO.Path.GetExtension(Name);

    Task<Stream> IFileEntry.OpenReadAsync()
    {
        using (CreateLogRegion())
        {
            return OnOpenReadAsync();
        }
    }

    Task<Stream> IFileEntry.OpenWriteAsync()
    {
        using (CreateLogRegion())
        {
            return OnOpenWriteAsync();
        }
    }

    protected abstract Task<Stream> OnOpenReadAsync();

    protected abstract Task<Stream> OnOpenWriteAsync();
}
