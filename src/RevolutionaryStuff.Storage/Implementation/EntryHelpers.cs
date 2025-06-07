namespace RevolutionaryStuff.Storage.Implementation;

public static class EntryHelpers
{
    public static async Task<IFolderEntry> GetParentFolderAsync(this IEntry entry)
    {
        var segments = StorageHelpers.GetPathSegments(entry.Path);
        var root = await entry.StorageProvider.OpenRootFolderAsync();
        if (segments.Length == 1)
            return root;
        else
        {
            var folderPath = segments.Take(segments.Length - 1).Join(StorageHelpers.ExternalFolderSeparator);
            return (IFolderEntry)await root.OpenAsync(folderPath);
        }
    }

    public static async Task DeleteAsync(this IEntry entry)
    {
        var segments = StorageHelpers.GetPathSegments(entry.Path);
        if (segments.Length == 0 && entry is IFolderEntry)
            throw new StorageProviderException(StorageProviderExceptionCodes.CannotDeleteRootFolder);
        var folder = await entry.GetParentFolderAsync();
        await folder.DeleteAsync(entry.Name);
    }
}
