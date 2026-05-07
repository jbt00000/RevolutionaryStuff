namespace RevolutionaryStuff.Applets.Blobs;

public record WriteBlobResult
{
    public required string StorageName { get; init; }
    public required string Name { get; init; }
    public required long Size { get; init; }
}

