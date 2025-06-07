namespace RevolutionaryStuff.Storage;

/// <summary>
/// The IFindResults interface provides a signature that is used
/// for getting search results, and moving forward and backward
/// through the pages of those search results.
/// </summary>
public interface IFindResults
{
    /// <summary>
    /// An <see cref="IList{T}"/> of <see cref="IEntry"/> results
    /// for a search.
    /// </summary>
    IList<IEntry> Entries { get; }

    /// <summary>
    /// Returns the next page of <see cref="IFindResults"/>.
    /// </summary>
    /// <returns>A task of <see cref="IFindResults"/>.</returns>
    Task<IFindResults> NextAsync();
}
