using RevolutionaryStuff.Applets.Blobs;

namespace RevolutionaryStuff.Applets.WebhookReceiverHost.Blobs;

public static class WebhookBlobWriterHelpers
{
    /// <summary>
    /// Path provider for <see cref="IWebhookAutoResponder"/>.
    /// Builds: {FolderHint}/{yyyy/MM/dd/HH}/{guid}/{fileName}
    /// where FolderHint is expected to be "{BaseFolderName}/{StorageFolderName}" as supplied by <see cref="WebhookAutoResponderConfig"/>.
    /// Register via:
    /// <code>services.AddBlobWriter&lt;TStorageProvider, TBlobWriter&gt;(WebhookBlobWriterHelpers.WebhookDiagnosticTimestampedPathProvider)</code>
    /// </summary>
    public static string WebhookDiagnosticTimestampedPathProvider(BlobWriterHelpers.PathProviderArgs ppa)
    {
        var now = ppa.settings?.Now ?? DateTimeOffset.UtcNow;
        var folder = ppa.settings?.FolderHint ?? "webhooks";
        return $"{folder}/{now:yyyy/MM/dd/HH}/{Guid.NewGuid()}/{ppa.fileName}";
    }
}
