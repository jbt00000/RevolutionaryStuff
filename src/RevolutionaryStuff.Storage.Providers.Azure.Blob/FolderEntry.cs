using System.IO;
using Azure.Storage.Blobs.Models;
using RevolutionaryStuff.Core.EncoderDecoders;
using RevolutionaryStuff.Storage.Implementation;
using RevolutionaryStuff.Storage.Implementation.Base;
using Traffk.StorageProviders.Providers.AzureBlobStorageProvider;
using static RevolutionaryStuff.Storage.IFolderEntry;

namespace RevolutionaryStuff.Storage.Providers.Azure.Blob;

internal class FolderEntry : BaseFolderEntry<AzureBlobStorageProvider>, IFolderEntry
{
    internal readonly string Prefix;

    internal FolderEntry(AzureBlobStorageProvider storageProvider, string prefix)
        : base(storageProvider)
    {
        Prefix = StorageProvider.NormalizeExternalFolderPath(prefix);
    }

    public override DateTimeOffset LastModified
        => throw new NotSupportedException();

    protected override string AbsolutePath
        => Prefix;

    private class ProgressHandler : IProgress<long>
    {
        public long BytesTransferred { get; private set; }
        void IProgress<long>.Report(long value) => BytesTransferred = value;
    }

    protected override async Task<IFileEntry> OnCreateFileAsync(string name, Stream st, CreateFileArgs args)
    {
        var res = await CreateFileAsync(name, st, args);
        return res.FileEntry;
    }

    private record CreateFileResult(IFileEntry FileEntry, BlobContentInfo BlobContentInfo, long? BytesUploaded);

    private async Task<CreateFileResult> CreateFileAsync(string name, Stream st, CreateFileArgs args)
    {
        var targetPath = StorageProvider.CombinePaths(AbsolutePath, name);
        var client = StorageProvider.ContainerClient.GetBlobClient(targetPath);
        if (await client.ExistsAsync())
            throw new StorageProviderException(StorageProviderExceptionCodes.CannotCreateFileWhenItAlreadyExists);
        BlobContentInfo blobContentInfo = null;
        ProgressHandler p = new();
        try
        {
            BlobHttpHeaders headers = null;
            if (args != null)
            {
                headers = new()
                {
                    ContentType = args.ContentType
                };
            }
            blobContentInfo = await client.UploadAsync(st ?? Stream.Null, headers, args.Metadata, null, p);
        }
        catch (Exception ex)
        {
            throw new StorageProviderException(StorageProviderExceptionCodes.CannotCreateFile, ex);
        }
        return new(new FileEntry(client, StorageProvider), blobContentInfo, p.BytesTransferred);
    }

    async Task IFolderEntry.WriteFileAsync(string path, Stream stream, CreateFileArgs args)
    {
        global::Azure.Response<bool> deleteIfExistsAsyncResponse = null;
        string targetPath = null;
        try
        {
            StorageProvider.RequiresForwardOnlyValidPath(path, nameof(path));
            Requires.ReadableStreamArg(stream);

            targetPath = StorageProvider.CombinePaths(AbsolutePath, path);
            var client = StorageProvider.ContainerClient.GetBlobClient(targetPath);
            deleteIfExistsAsyncResponse = await client.DeleteIfExistsAsync();

            StorageProvider.RequiresForwardOnlyValidPath(path, nameof(path));
            var res = await CreateFileAsync(path, stream, args);
            var fileEntry = res.FileEntry;
            //var fileEntry = await I.CreateFileAsync(path, stream, args);

            var size = res.BytesUploaded;
            if (size == null || size == 0)
            {
                try
                {
                    size = stream.Length;
                }
                catch (NotSupportedException)
                {
                    try
                    {
                        size = fileEntry.Length;
                    }
                    catch (Exception) { }
                }
            }

            LogInformation(
                nameof(IFolderEntry.WriteFileAsync) + " success with file of {size} to {path} yielding {contentHash}; {existingDeleted}",
                size, targetPath,
                res.BlobContentInfo?.ContentHash == null ? null : Base16.Encode(res.BlobContentInfo?.ContentHash),
                deleteIfExistsAsyncResponse?.Value);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(IFolderEntry.WriteFileAsync) + " failure of file of {size} to {path}; {existingDeleted}", stream.Length, targetPath, deleteIfExistsAsyncResponse?.Value);
            throw;
        }
    }

    protected override IStorageProvider OnCreateScopedStorageProvider()
        => new AzureBlobStorageProvider(StorageProvider, AbsolutePath);

    protected override async Task<IFolderEntry> OnCreateFolderAsync(string name)
    {
        var targetPath = StorageProvider.CombinePaths(AbsolutePath, name);
        if (StorageProvider.DfsClientUrl != null)
        {
            try
            {
                var u = new Uri($"{StorageProvider.DfsClientUrl}/{targetPath}");
                var client = StorageProvider.CreateDataLakeDirectoryClient(u);
                await client.CreateAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
        var folder = new FolderEntry(StorageProvider, targetPath);
        return folder;
    }

    protected override Task OnDeleteAsync(string name)
    {
        var targetPath = StorageProvider.CombinePaths(AbsolutePath, name);
        var blobClient = StorageProvider.ContainerClient.GetBlobClient(targetPath);

        if (blobClient.Exists())
            return blobClient.DeleteAsync();
        if (StorageProvider.DfsClientUrl != null)
        {
            var u = new Uri($"{StorageProvider.DfsClientUrl}/{targetPath}");
            var client = StorageProvider.CreateDataLakeDirectoryClient(u);
            return client.DeleteAsync();
        }
        else
        {
            throw new NotSupportedException($"Must have enabled {nameof(AzureBlobStorageProvider.Config.IsHierarchical)} in the config");
        }
    }

    protected override async Task<IEntry> OnOpenAsync(string name)
    {
        var targetPath = StorageProvider.CombinePaths(AbsolutePath, name);
        var blobClient = StorageProvider.ContainerClient.GetBlobClient(targetPath);

        if (await blobClient.ExistsAsync())
        {
            var file = new FileEntry(blobClient, StorageProvider);
            await file.InitializeAsync();
            if (!await file.IsHierarchicalFolderAsync())
                return file;
        }
        if (StorageProvider.DfsClientUrl != null)
        {
            var u = new Uri($"{StorageProvider.DfsClientUrl}/{targetPath}");
            var directoryClient = StorageProvider.CreateDataLakeDirectoryClient(u);
            if ((await directoryClient.ExistsAsync()).Value)
            {
                var props = (await directoryClient.GetPropertiesAsync()).Value;
                if (props.Metadata.TryGetValue(MetadataFieldNames.IsFolder, out var sv) && Parse.ParseBool(sv))
                {
                    var folder = new FolderEntry(StorageProvider, targetPath);
                    return folder;
                }
            }
        }
        return null;
        //throw new StorageProviderException(StorageProviderExceptionCodes.DoesNotExist);
    }

    protected override async Task<IFindResults> OnFindAsync(IFindCriteria criteria)
    {
        var res = new FindResults(this, criteria);
        await res.InitializeAsync();
        return res;
    }

    private class FindResults : IFindResults
    {
        private readonly FolderEntry Folder;
        private readonly IFindCriteria Criteria;
        private readonly string ContinuationToken;

        public FindResults(FolderEntry folder, IFindCriteria criteria)
            : this(folder, criteria, null)
        { }

        private FindResults(FolderEntry folder, IFindCriteria criteria, string continuationToken)
        {
            Folder = folder;
            Criteria = criteria;
            ContinuationToken = continuationToken;
        }

        internal async Task InitializeAsync()
        {
            var storageProvider = Folder.StorageProvider;
            NextToken = ContinuationToken;
            var prefix = Folder.AbsolutePath;
            var delim = Criteria.NestingOption == MatchNestingOptionEnum.CurrentFolderAndChildFolders ? null : storageProvider.InternalFolderSeparatorString;
Iterate:
            await foreach (var page in storageProvider.ContainerClient.GetBlobsByHierarchyAsync(delimiter: delim, prefix: prefix).AsPages(NextToken))
            {
                var cnt = 0;
                foreach (var item in page.Values)
                {
                    ++cnt;
                    Stuff.NoOp(item);
                    if (item.IsBlob)
                    {
                        var name = StorageHelpers.GetPathSegments(item.Blob.Name).Last();
                        if (!Criteria.IsMatch(name)) continue;
                        if (Criteria.NestingOption == MatchNestingOptionEnum.CurrentFolderAndChildFolders && item.Blob.Properties.ContentLength == 0 && !item.Blob.Properties.ContentHash.NullSafeAny())
                            Entries.Add(new FolderEntry(storageProvider, item.Blob.Name));
                        else
                        {
                            var client = storageProvider.ContainerClient.GetBlobClient(item.Blob.Name);
                            Entries.Add(new FileEntry(client, storageProvider));
                        }
                    }
                    else if (item.IsPrefix)
                    {
                        var name = item.Prefix.Split(new[] { storageProvider.InternalFolderSeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Last();
                        if (!Criteria.IsMatch(name)) continue;
                        Entries.Add(new FolderEntry(storageProvider, item.Prefix));
                    }
                    else
                    {
                        throw new UnexpectedSwitchValueException($"{item.Prefix} is neither a blob nor a folder.  huh?");
                    }
                }
                NextToken = page.ContinuationToken;
                if (cnt > 0 && Entries.Count == 0 && IsNextTokenValid)
                    goto Iterate;
            }
        }

        private string NextToken;

        private bool IsNextTokenValid => !string.IsNullOrEmpty(NextToken);

        async Task<IFindResults> IFindResults.NextAsync()
        {
            if (!IsNextTokenValid) return StorageHelpers.NoFindResults;
            var fr = new FindResults(Folder, Criteria, NextToken);
            await fr.InitializeAsync();
            return fr;
        }

        private readonly List<IEntry> Entries = [];
        IList<IEntry> IFindResults.Entries => Entries;
    }
}
