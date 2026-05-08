namespace RevolutionaryStuff.Applets.Blobs;

public record WriteBlobSettings
{
    /// <summary>
    /// A content type to record if the storage provider supports it
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Optional properties for an IUserPropertyStore implementation
    /// </summary>
    public IDictionary<string, string>? Metadata { get; set; }

    public DateTimeOffset? Now { get; init; }

    /// <summary>
    /// An optional caller-provided folder scope hint for use by the registered IBlobWriter path provider.
    /// </summary>
    public string? FolderHint { get; init; }
}

