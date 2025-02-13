using System.IO;

namespace RevolutionaryStuff.Core;

public static class UrlHelpers
{
    public static IList<string> GetLocalPathParts(this Uri u, bool removeEmptyEntries = true)
        => u.LocalPath.Split(new[] { Path.AltDirectorySeparatorChar }, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

    public static string GetFileNameSegment(this Uri u)
        => u.GetLocalPathParts().Last();
}
