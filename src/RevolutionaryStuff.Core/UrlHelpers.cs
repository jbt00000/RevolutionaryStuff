using System.Collections.Generic;
using System.IO;

namespace RevolutionaryStuff.Core;

public static class UrlHelpers
{
    public static IList<string> GetLocalPathParts(this Uri u, bool removeEmptyEnties = true)
        => u.LocalPath.Split(new[] { Path.AltDirectorySeparatorChar }, removeEmptyEnties ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

    public static string GetFileNameSegment(this Uri u)
        => u.GetLocalPathParts().Last();
}
