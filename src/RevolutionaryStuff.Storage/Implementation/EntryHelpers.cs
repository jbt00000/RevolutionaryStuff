namespace RevolutionaryStuff.Storage.Implementation;

public static class EntryHelpers
{
    extension(IEntry source)
    {
        public async Task<IFolderEntry> GetParentFolderAsync()
        {
            var segments = StorageHelpers.GetPathSegments(source.Path);
            var root = await source.StorageProvider.OpenRootFolderAsync();
            if (segments.Length == 1)
                return root;
            else
            {
                var folderPath = segments.Take(segments.Length - 1).Join(StorageHelpers.ExternalFolderSeparator);
                return (IFolderEntry)await root.OpenAsync(folderPath);
            }
        }
        public async Task DeleteAsync()
        {
            var segments = StorageHelpers.GetPathSegments(source.Path);
            if (segments.Length == 0 && source is IFolderEntry)
                throw new StorageProviderException(StorageProviderExceptionCodes.CannotDeleteRootFolder);
            var folder = await source.GetParentFolderAsync();
            await folder.DeleteAsync(source.Name);
        }
    }


}
