using System.IO;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides helpers for accessing path segments in a URI's local path.
/// </summary>
public static class UrlHelpers
{
    extension(Uri source)
    {
        /// <summary>
        /// Splits a URI's local path into segments.
        /// </summary>
        /// <param name="source">The URI whose local path is split.</param>
        /// <param name="removeEmptyEntries">Whether to omit empty path segments.</param>
        /// <returns>The local path segments.</returns>
        public IList<string> GetLocalPathParts(bool removeEmptyEntries = true)
            => source.LocalPath.Split(new[] { Path.AltDirectorySeparatorChar }, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        /// <summary>
        /// Gets the final segment of a URI's local path.
        /// </summary>
        /// <param name="source">The URI whose final local path segment is returned.</param>
        /// <returns>The final non-empty local path segment.</returns>
        public string GetFileNameSegment()
            => source.GetLocalPathParts().Last();
    }


}
