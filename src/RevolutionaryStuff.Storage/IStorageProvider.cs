using System.IO;

namespace RevolutionaryStuff.Storage;

/// <summary>
/// The IStorageProvider interface provides the base signature
/// that all provider must implement. All providers must have
/// a root folder.
/// </summary>
public interface IStorageProvider : IDisposable
{
    /// <summary>
    /// The scoped folder that acts as the root for this storage provider
    /// </summary>
    /// <returns>A folder entry that acts as the root folder</returns>
    Task<IFolderEntry> OpenRootFolderAsync();

    #region Default Implementations

    async Task<Stream> OpenFileReadStreamAsync(string path)
    {
        var file = await OpenFileAsync(path);
        return file != null ? await file.OpenReadAsync() : null;
    }

    async Task<IEntry> OpenAsync(string path)
    {
        Requires.Text(path);
        var root = await OpenRootFolderAsync();
        return await root.OpenAsync(path);
    }

    async Task<IFileEntry> OpenFileAsync(string path)
        => await OpenAsync(path) as IFileEntry;

    async Task<IFolderEntry> OpenFolderAsync(string path)
        => await OpenAsync(path) as IFolderEntry;

    async Task<IFolderEntry> OpenOrCreateFolderAsync(string path)
    {
        var root = await OpenRootFolderAsync();
        return await root.OpenOrCreateFolderAsync(path);
    }

    #endregion
}
