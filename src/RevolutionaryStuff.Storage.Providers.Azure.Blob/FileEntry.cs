using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using RevolutionaryStuff.Core.Streams;
using RevolutionaryStuff.Storage.Implementation;
using RevolutionaryStuff.Storage.Implementation.Base;
using Traffk.StorageProviders.Providers.AzureBlobStorageProvider;

namespace RevolutionaryStuff.Storage.Providers.Azure.Blob;

internal class FileEntry : BaseFileEntry<AzureBlobStorageProvider>, IWebAccess, IUserPropertyStore
{
    private readonly BlobClient Client;

    internal FileEntry(BlobClient client, AzureBlobStorageProvider storageProvider)
        : base(storageProvider)
    {
        Client = client;
        Name_p = StorageHelpers.GetPathSegments(Client.Name).Last();
    }

    public override string Name => Name_p;
    private readonly string Name_p;

    private BlobProperties Properties_p;

    private async Task<BlobProperties> GetPropertiesAsync()
    {
        if (IsDirty)
            await InitializeAsync();
        return Properties_p;
    }

    private BlobProperties Properties
        => GetPropertiesAsync().ExecuteSynchronously();

    internal async Task InitializeAsync(bool force = false)
    {
        if (IsDirty || force)
        {
            var resp = await Client.GetPropertiesAsync();
            Properties_p = resp.Value;
            IsDirty = false;
        }
    }

    internal async Task<bool> IsHierarchicalFolderAsync()
    {
        var res = await Client.ExistsAsync();
        if (res.Value)
        {
            var props = (await Client.GetPropertiesAsync()).Value;
            return props.Metadata.TryGetValue(MetadataFieldNames.IsFolder, out var sv) && Parse.ParseBool(sv);
        }
        return false;
    }

    public override long Length
        => Properties.ContentLength;

    public override DateTimeOffset LastModified
        => Properties.LastModified;

    protected override string AbsolutePath
        => StorageHelpers.ExternalFolderSeparator + StorageHelpers.GetPathSegments(Client.Uri.LocalPath).Skip(1).Join(StorageHelpers.ExternalFolderSeparator);

    protected override async Task<Stream> OnOpenReadAsync()
    {
        await InitializeAsync();
        var rawStream = File.Create(System.IO.Path.GetTempFileName(), 1024 * 128, FileOptions.DeleteOnClose);
        await Client.DownloadToAsync(rawStream);
        rawStream.Position = 0;
        return rawStream;
    }

    protected override async Task<Stream> OnOpenWriteAsync()
    {
        await InitializeAsync();
        var rawStream = File.Create(System.IO.Path.GetTempFileName(), 1024 * 128, FileOptions.DeleteOnClose);
        if (Length > 0)
        {
            await Client.DownloadToAsync(rawStream);
            rawStream.Position = 0;
        }
        var mst = new MonitoredStream(rawStream);
        mst.DirtyEvent += (s, e) =>
        {
            UploadOnClose = true;
            IsDirty = true;
        };
        mst.CloseEvent += (s, e) =>
        {
            if (!UploadOnClose) return;
            rawStream.Position = 0;
            Client.Upload(rawStream, overwrite: true);
            IsDirty = true;
            UploadOnClose = false;
        };
        return mst;
    }

    async Task<Uri> IWebAccess.GenerateExternalUrlAsync(ExternalAccessSettings settings)
    {
        settings ??= ExternalAccessSettings.CreateDefaultSettings();
        var b = AzureBlobStorageProviderHelpers.CreateBuilder(settings);
        b.BlobContainerName = Client.BlobContainerName;
        b.Resource = "b";
        b.BlobName = Client.Name;
        if (settings.OverrideContentType != null)
            b.ContentType = settings.OverrideContentType;
        else if (settings.SetContentTypeBasedOnFileExtension)
        {
            var ext = ((IFileEntry)this)?.Extension;
            if (ext != null)
            {
                var m = MimeType.FindByExtension(ext);
                if (m != null)
                    b.ContentType = m.PrimaryContentType;
            }
        }

        BlobSasQueryParameters qp;
        if (StorageProvider.StorageSharedKeyCredential != null)
            qp = b.ToSasQueryParameters(StorageProvider.StorageSharedKeyCredential);
        else
        {
            var userDelegationKey = await StorageProvider.GetUserDelegationKeyAsync(settings.CalculateExpiresAt());
            qp = b.ToSasQueryParameters(userDelegationKey, Client.AccountName);
        }
        var u = new Uri($"{Client.Uri}?{qp}");
        return u;
    }

    async Task<IDictionary<string, string>> IUserPropertyStore.GetUserPropertiesAsync()
        => (await GetPropertiesAsync()).Metadata;

    async Task IUserPropertyStore.SetUserPropertiesAsync(IDictionary<string, string> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        await Client.SetMetadataAsync(properties);
        IsDirty = true;
    }

    private bool IsDirty = true;
    private bool UploadOnClose;
    /*
    protected override async Task OnRenameAsync(string newName)
    {
        await StorageProvider.RenameAsync(Client.Uri, newName, false, Length);
        //The dumb ass operation clones the file to the new path instead of actually renaming/moving it.
        await OnDeleteAsync();
    }
    */
}
