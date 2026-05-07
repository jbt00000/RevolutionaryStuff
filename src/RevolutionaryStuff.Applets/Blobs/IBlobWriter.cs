namespace RevolutionaryStuff.Applets.Blobs;

public interface IBlobWriter
{
    Task<WriteBlobResult> WriteBlobAsync(string name, System.IO.Stream st, WriteBlobSettings? settings = null);
}

