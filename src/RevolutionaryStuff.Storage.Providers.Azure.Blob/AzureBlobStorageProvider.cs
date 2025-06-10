using Azure.Core;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Storage.Implementation;
using RevolutionaryStuff.Storage.Implementation.Base;

namespace RevolutionaryStuff.Storage.Providers.Azure.Blob;

public partial class AzureBlobStorageProvider : BaseStorageProvider, IAzureBlobStorageProvider
{
    public const string StorageProviderName = "AzureBlobStorageProviderStorageProvider";
    private const string DefaultRootPath = "/";
    private readonly IOptions<Config> ConfigOptions;
    private readonly IConnectionStringProvider ConnectionStringProvider;

    internal readonly TokenCredential TokenCredential;
    internal readonly StorageSharedKeyCredential StorageSharedKeyCredential;

    internal readonly Uri BlobClientUrl;
    internal readonly Uri DfsClientUrl;
    internal readonly BlobServiceClient ServiceClient;
    internal readonly BlobContainerClient ContainerClient;


    private global::Azure.Storage.Blobs.Models.UserDelegationKey UserDelegationKey;
    private DateTimeOffset UserDelegationKeyExpiration;
    private TimeSpan UserDelegationKeyMaxExpiration = TimeSpan.FromDays(7);

    internal async Task<global::Azure.Storage.Blobs.Models.UserDelegationKey> GetUserDelegationKeyAsync(DateTimeOffset expiresOn)
    {
        if (UserDelegationKey == null || expiresOn > UserDelegationKeyExpiration)
        {
            expiresOn = DateTimeOffset.UtcNow.Add(UserDelegationKeyMaxExpiration);
            UserDelegationKey = await ServiceClient.GetUserDelegationKeyAsync(null, expiresOn);
            UserDelegationKeyExpiration = expiresOn;
        }
        return UserDelegationKey;
    }

    public AzureBlobStorageProvider(IOptions<Config> configOptions, IConnectionStringProvider connectionStringProvider, ILogger<AzureBlobStorageProvider> logger)
        : this(configOptions, connectionStringProvider, DefaultRootPath, logger)
    { }

    internal AzureBlobStorageProvider(AzureBlobStorageProvider storageProvider, string rootPath)
        : this(storageProvider.ConfigOptions, storageProvider.ConnectionStringProvider, rootPath, storageProvider.Logger)
    { }

    private AzureBlobStorageProvider(IOptions<Config> configOptions, IConnectionStringProvider connectionStringProvider, string rootPath, ILogger logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(connectionStringProvider);
        ArgumentNullException.ThrowIfNull(configOptions);

        ConfigOptions = configOptions;
        ConnectionStringProvider = connectionStringProvider;
        var config = ConfigOptions.Value;
        Requires.Valid(config);

        InternalFolderSeparatorChar = '/';
        AbsolutePath = NormalizeExternalFolderPath(rootPath ?? StorageHelpers.RootPath);
        CaseSensitive = config.CaseSensitive;
        var connectionString = connectionStringProvider.GetConnectionString(config.ConnectionStringName);

        if (config.AuthenticateWithWithDefaultAzureCredentials)
        {
            TokenCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions());
            ServiceClient = new BlobServiceClient(new Uri(connectionString), TokenCredential);
            ContainerClient = ServiceClient.GetBlobContainerClient(config.ContainerName);
        }
        else
        {
            var d = ConnectionStringHelpers.ConnectionStringToDictionary(connectionString);
            var accountName = d["AccountName"];
            var accountKey = d["AccountKey"];
            Requires.Text(accountName);
            Requires.Text(accountKey);
            StorageSharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);
            ContainerClient = new BlobContainerClient(connectionString, config.ContainerName);
        }
        BlobClientUrl = ContainerClient.Uri;
        if (config.IsHierarchical)
        {
            DfsClientUrl = new Uri($"https://{ContainerClient.AccountName}.dfs.core.windows.net/{config.ContainerName}");
        }
        RootFolder = new FolderEntry(this, AbsolutePath);
    }

    internal DataLakeDirectoryClient CreateDataLakeDirectoryClient(Uri u)
    {
        return TokenCredential != null
            ? new DataLakeDirectoryClient(u, TokenCredential)
            : StorageSharedKeyCredential != null
                ? new DataLakeDirectoryClient(u, StorageSharedKeyCredential)
                : throw new NotSupportedException("No credentials available");
    }

    public override string CombinePaths(string basePath, string addedPath)
    {
        var ret = base.CombinePaths(basePath, addedPath);
        if (ret.StartsWith(InternalFolderSeparatorString))
        {
            ret = ret[1..];
        }
        return ret;
    }

    protected override Task<IFolderEntry> OnOpenRootFolderAsync()
        => Task.FromResult(RootFolder);

    private IFolderEntry RootFolder { get; }

#if false
    internal async Task RenameAsync(Uri sourceAbsoluteUrl, string destinationRelativePath, bool isFolder, long? itemSize = null)
    {
        var rel = StorageHelpers.CreateRelativePath(sourceAbsoluteUrl, isFolder, destinationRelativePath);
        ThrowIfNotWithinTree(rel);
        var destinationAbsoluteUrl = new Uri(sourceAbsoluteUrl, rel);
        await AzureRestHelpers.RenameAsync(
            ARR.AccountName, ARR.Base64Credentials, sourceAbsoluteUrl, destinationAbsoluteUrl,
            new Dictionary<string, string> { { "x-ms-blob-type", "BlockBlob" } },
            itemSize);
    }
#endif

    public Task<Uri> GenerateExternalUrlAsync(string path, ExternalAccessSettings settings)
    {
        var targetPath = CombinePaths(AbsolutePath, path);
        var blobClient = ContainerClient.GetBlobClient(targetPath);
        var file = new FileEntry(blobClient, this);
        return ((IWebAccess)file).GenerateExternalUrlAsync(settings);
    }
}
