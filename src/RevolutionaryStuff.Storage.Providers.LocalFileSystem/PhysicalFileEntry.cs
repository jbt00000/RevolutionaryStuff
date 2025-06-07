using System.IO;
using RevolutionaryStuff.Storage.Implementation.Base;
using Traffk.StorageProviders.Providers.PhysicalStorage;

namespace RevolutionaryStuff.Storage.Providers.LocalFileSystem;

internal class PhysicalFileEntry : BaseFileEntry<PhysicalStorageProvider>
{
    public PhysicalFileEntry(PhysicalStorageProvider storageProvider, FileInfo fileInfo)
        : base(storageProvider)
    {
        FileInfo = fileInfo;
    }

    protected override string AbsolutePath
        => FileInfo.FullName;

    public override DateTimeOffset LastModified =>
        new(FileInfo.LastWriteTimeUtc);

    private FileInfo FileInfo
    {
        get
        {
            if (ForceFileInfoRefresh)
                field?.Refresh();
            return field;
        }
        set;
    }
    private bool ForceFileInfoRefresh;

    public override long Length
        => FileInfo.Length;

    protected override Task<Stream> OnOpenReadAsync()
        => Task.FromResult((Stream)FileInfo.OpenRead());

    protected override Task<Stream> OnOpenWriteAsync()
    {
        ForceFileInfoRefresh = true;
        return Task.FromResult((Stream)FileInfo.OpenWrite());
    }
}
