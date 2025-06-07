namespace RevolutionaryStuff.Storage.Providers.Azure.Blob;

public interface IAzureBlobStorageProvider : IStorageProvider
{
    Task<Uri> GenerateExternalUrlAsync(string path, ExternalAccessSettings settings);
}
