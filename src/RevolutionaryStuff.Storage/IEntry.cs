namespace RevolutionaryStuff.Storage;

/// <summary>
/// The IEntry interface is used to represent a file or folder resource
/// in any of the various provider implementations.
/// </summary>
public interface IEntry
{
    /// <summary>
    /// The storage provider that owns this entry
    /// </summary>
    IStorageProvider StorageProvider { get; }

    /// <summary>
    /// When the file was last modified.
    /// </summary>
    DateTimeOffset LastModified { get; }

    /// <summary>
    /// The name of the file or directory, not including any path.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The path of this entry relative to the root
    /// </summary>
    string Path { get; }

    #region Default Implementations

    /// <summary>
    /// A helper method that determines if the <see cref="IEntry" />
    /// is an <see cref="IFileEntry" />.
    /// </summary>
    /// <param name="entry">The <see cref="IEntry" /> to evaluate.</param>
    /// <returns>true if the <see cref="IEntry" /> is an <see cref="IFileEntry" />.</returns>
    bool IsFileEntry()
        => this is IFileEntry;

    /// <summary>
    /// A helper method that determines if the <see cref="IEntry" />
    /// is an <see cref="IFolderEntry" />.
    /// </summary>
    /// <param name="entry">The <see cref="IEntry" /> to evaluate.</param>
    /// <returns>true if the <see cref="IEntry" /> is an <see cref="IFolderEntry" />.</returns>
    bool IsFolderEntry()
        => this is IFolderEntry;


    #endregion
}
