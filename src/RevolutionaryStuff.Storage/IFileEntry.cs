using System.IO;

namespace RevolutionaryStuff.Storage;

/// <summary>
/// The IFileEntry interface provides some properties that extend
/// <see cref="IEntry"/> that are useful for for dealing with a
/// file-like implementation across the providers. It provides
/// properties like the Length of the <see cref="IEntry"/>, and
/// methods to read and write to the entry.
/// </summary>
public interface IFileEntry : IEntry
{
    /// <summary>
    /// The length of the file in bytes, or -1 for non-existing files.
    /// </summary>
    long Length { get; }

    /// <summary>
    /// Opens a readable stream for this entry.
    /// </summary>
    /// <returns>A readable <see cref="Stream"/>.</returns>
    Task<Stream> OpenReadAsync();

    /// <summary>
    /// Opens a writable stream for this entry.
    /// </summary>
    /// <returns>A writable <see cref="Stream"/>.</returns>
    Task<Stream> OpenWriteAsync();

    /// <summary>
    /// The file extension of the filename
    /// </summary>
    string Extension { get; }
}
