namespace RevolutionaryStuff.Storage.Providers.Azure.Blob;

internal static class AzureBlobStorageProviderHelpers
{
    public static global::Azure.Storage.Sas.BlobSasPermissions? GetBlobSasPermissions(ExternalAccessSettings.AccessTypeEnum externalAccessSettings)
        => externalAccessSettings switch
        {
            ExternalAccessSettings.AccessTypeEnum.Read => global::Azure.Storage.Sas
                .BlobSasPermissions.Read,
            _ => null
        };

    public static global::Azure.Storage.Sas.BlobSasBuilder CreateBuilder(ExternalAccessSettings externalAccessSettings)
    {
        var expires = externalAccessSettings.CalculateExpiresAt();
        var p = GetBlobSasPermissions(externalAccessSettings.AccessType);
        global::Azure.Storage.Sas.BlobSasBuilder b = new()
        {
            ExpiresOn = expires
        };
        if (p != null)
            b.SetPermissions(p.Value);
        return b;
    }
}
