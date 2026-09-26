using System.IO;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides helpers for accessing path segments in a URI's local path.
/// </summary>
public static class UrlHelpers
{
    /// <summary>
    /// Splits a URI's local path into segments.
    /// </summary>
    /// <param name="u">The URI whose local path is split.</param>
    /// <param name="removeEmptyEntries">Whether to omit empty path segments.</param>
    /// <returns>The local path segments.</returns>
    public static IList<string> GetLocalPathParts(this Uri u, bool removeEmptyEntries = true)
        => u.LocalPath.Split(new[] { Path.AltDirectorySeparatorChar }, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

    /// <summary>
    /// Gets the final segment of a URI's local path.
    /// </summary>
    /// <param name="u">The URI whose final local path segment is returned.</param>
    /// <returns>The final non-empty local path segment.</returns>
    public static string GetFileNameSegment(this Uri u)
        => u.GetLocalPathParts().Last();
}
