using System.IO;
using RevolutionaryStuff.Storage.Implementation;

namespace RevolutionaryStuff.Storage;

public interface IFolderEntry : IEntry
{
    /// <summary>
    /// The FindAsync method provides a mechanism for retrieving subfolders and
    /// files using a find criteria.
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    Task<IFindResults> FindAsync(IFindCriteria criteria = null);

    /// <summary>
    /// Create a file asynchronously.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <returns>A <see cref="IFileEntry" /> representing the new file.</returns>
    Task<IFileEntry> CreateFileAsync(string path, Stream st = null, CreateFileArgs args = null);

    class CreateFileArgs
    {
        /// <summary>
        /// A content type to record if the storage provider supports it
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Optional properties for an IUserPropertyStore implementation
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; }
    }

    /// <summary>
    /// Create a folder asynchronously.
    /// </summary>
    /// <param name="name">The name of the folder.</param>
    /// <returns>A <see cref="IFolderEntry" /> representing the new folder.</returns>
    Task<IFolderEntry> CreateFolderAsync(string path);

    Task<IEntry> OpenAsync(string path);

    /// <summary>
    /// Deletes this entry
    /// </summary>
    /// <returns>A waitable completion task</returns>
    Task DeleteAsync(string path);
    IStorageProvider CreateScopedStorageProvider();

    #region Default Implementations

    async Task WriteFileAsync(string path, byte[] buffer, CreateFileArgs args = null)
    {
        using var st = new MemoryStream(buffer, false);
        await WriteFileAsync(path, st, args);
    }

    async Task WriteFileAsync(string path, Stream st, CreateFileArgs args = null)
    {
        await DeleteAsync(path);
        await CreateFileAsync(path, st, args);
    }

    async Task<Stream> OpenFileReadStreamAsync(string path)
    {
        Requires.Text(path);
        var file = await OpenFileAsync(path);
        return file != null ? await file.OpenReadAsync() : null;
    }

    async Task<IFolderEntry> OpenFolderAsync(string path)
    {
        Requires.Text(path);
        return await OpenAsync(path) as IFolderEntry;
    }

    async Task<IFileEntry> OpenFileAsync(string path)
    {
        Requires.Text(path);
        return await OpenAsync(path) as IFileEntry;
    }

    async Task<IFolderEntry> OpenOrCreateFolderAsync(string name)
    {
        var f = await OpenFolderAsync(name) ?? await CreateFolderAsync(name);
        return f;
    }

    async Task<IList<IEntry>> GetAllEntriesAsync(bool recurse = false)
    {
        var items = new List<IEntry>();
        var res = await FindAsync(recurse ? FindCriteria.AllItemsAllFolderFindCriteria : FindCriteria.AllItemsCurrentFolderFindCriteria);
Again:
        if (res != null && res.Entries != null && res.Entries.Count > 0)
        {
            items.AddRange(res.Entries);
            res = await res.NextAsync();
            goto Again;
        }
        return items;
    }

    #endregion
}
