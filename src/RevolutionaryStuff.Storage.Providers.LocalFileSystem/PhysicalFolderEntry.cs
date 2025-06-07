using System.IO;
using RevolutionaryStuff.Storage.Implementation.Base;
using Traffk.StorageProviders.Providers.PhysicalStorage;
using static RevolutionaryStuff.Storage.IFolderEntry;

namespace RevolutionaryStuff.Storage.Providers.LocalFileSystem;

internal class PhysicalFolderEntry : BaseFolderEntry<PhysicalStorageProvider>
{
    private readonly DirectoryInfo DirectoryInfo;
    public override DateTimeOffset LastModified
        => new(DirectoryInfo.LastWriteTimeUtc);

    protected override string AbsolutePath
        => DirectoryInfo.FullName;

    public PhysicalFolderEntry(PhysicalStorageProvider storageProvider, DirectoryInfo directoryInfo)
        : base(storageProvider)
    {
        DirectoryInfo = directoryInfo;
    }

    protected override IStorageProvider OnCreateScopedStorageProvider()
        => new PhysicalStorageProvider(StorageProvider, AbsolutePath);

    protected override async Task<IFileEntry> OnCreateFileAsync(string name, Stream st, CreateFileArgs args)
    {
        var targetPath = StorageProvider.CombinePaths(AbsolutePath, name);
        if (File.Exists(targetPath))
            throw new StorageProviderException(StorageProviderExceptionCodes.CannotCreateFileWhenItAlreadyExists);
        try
        {
            var dir = System.IO.Path.GetDirectoryName(targetPath);
            LogWarning("Creating dir {path}", dir);
            Directory.CreateDirectory(dir);
            LogWarning("Creating file {path}", targetPath);
            using var dst = File.Create(targetPath);
            if (st != null)
                await st.CopyToAsync(dst);
        }
        catch (Exception ex)
        {
            throw new StorageProviderException(StorageProviderExceptionCodes.CannotCreateFile, ex);
        }

        var file = new PhysicalFileEntry(StorageProvider, new FileInfo(targetPath));
        return file;
    }

    protected override Task<IFolderEntry> OnCreateFolderAsync(string name)
    {
        var targetPath = StorageProvider.CombinePaths(AbsolutePath, name);
        LogWarning("Creating dir {path}", targetPath);
        Directory.CreateDirectory(targetPath);
        var di = new DirectoryInfo(targetPath);
        return Task.FromResult<IFolderEntry>(new PhysicalFolderEntry(StorageProvider, di));
    }

    protected override Task OnDeleteAsync(string name)
    {
        var targetPath = StorageProvider.CombinePaths(AbsolutePath, name);
        LogWarning("Deleting {path}", targetPath);
        if (File.Exists(targetPath))
            File.Delete(targetPath);
        else if (Directory.Exists(targetPath))
        {
            Directory.Delete(targetPath, true);
        }
        return Task.CompletedTask;
    }

    protected override Task<IFindResults> OnFindAsync(IFindCriteria criteria)
        => Task.FromResult<IFindResults>(
            new PhysicalFindResults(
                StorageProvider,
                AbsolutePath,
                criteria
            )
        );

    protected override Task<IEntry> OnOpenAsync(string name)
    {
        IEntry ret;
        var path = StorageProvider.CombinePaths(AbsolutePath, name);
        LogWarning("Openning {path}", path);
        ret = Directory.Exists(path)
            ? new PhysicalFolderEntry(StorageProvider, new DirectoryInfo(path))
            : File.Exists(path) ? new PhysicalFileEntry(StorageProvider, new FileInfo(path)) : (IEntry)null;
        return Task.FromResult(ret);
    }
}
