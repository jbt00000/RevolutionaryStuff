using System.IO;
using static RevolutionaryStuff.Storage.IFolderEntry;

namespace RevolutionaryStuff.Storage.Implementation.Base;

public abstract class BaseFolderEntry<SP> : BaseEntry<SP>, IFolderEntry
    where SP : BaseStorageProvider
{
    protected readonly IFolderEntry I;

    protected BaseFolderEntry(SP storageProvider)
        : base(storageProvider)
    {
        I = this;
    }

    private static readonly CreateFileArgs EmptyCreateFileArgs = new();
    Task<IFileEntry> IFolderEntry.CreateFileAsync(string path, Stream st, CreateFileArgs args)
    {
        using (CreateLogRegion(path))
        {
            StorageProvider.RequiresForwardOnlyValidPath(path, nameof(path));
            return OnCreateFileAsync(path, st, args ?? EmptyCreateFileArgs);
        }
    }

    Task<IFolderEntry> IFolderEntry.CreateFolderAsync(string path)
    {
        using (CreateLogRegion(path))
        {
            StorageProvider.RequiresForwardOnlyValidPath(path, nameof(path));
            return OnCreateFolderAsync(path);
        }
    }

    Task<IFindResults> IFolderEntry.FindAsync(IFindCriteria criteria)
    {
        using (CreateLogRegion())
        {
            ArgumentNullException.ThrowIfNull(criteria);
            return OnFindAsync(criteria);
        }
    }

    async Task<IEntry> IFolderEntry.OpenAsync(string path)
    {
        using (CreateLogRegion(path))
        {
            StorageProvider.RequiresForwardOnlyValidPath(path, nameof(path));
            try
            {
                return await OnOpenAsync(path);
            }
            catch (StorageProviderException spe) when (spe.Code == StorageProviderExceptionCodes.DoesNotExist)
            {
                return null;
            }
        }
    }

    Task IFolderEntry.DeleteAsync(string path)
    {
        using (CreateLogRegion(path))
        {
            StorageProvider.RequiresForwardOnlyValidPath(path, nameof(path));
            return OnDeleteAsync(path);
        }
    }

    protected abstract Task<IFileEntry> OnCreateFileAsync(string name, Stream st, CreateFileArgs args);

    protected abstract Task<IFolderEntry> OnCreateFolderAsync(string name);

    protected abstract Task<IFindResults> OnFindAsync(IFindCriteria criteria);

    protected abstract Task<IEntry> OnOpenAsync(string name);

    protected abstract Task OnDeleteAsync(string name);


    IStorageProvider IFolderEntry.CreateScopedStorageProvider()
    {
        using (CreateLogRegion())
        {
            return OnCreateScopedStorageProvider();
        }
    }

    protected abstract IStorageProvider OnCreateScopedStorageProvider();
}
