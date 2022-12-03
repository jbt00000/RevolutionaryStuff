using System.IO;
using System.Reflection;

namespace RevolutionaryStuff.Core;

public static class ResourceHelpers
{
    /// <summary>
    /// Get an embedded resource as a stream
    /// </summary>
    /// <param name="name">The unqualified name of the resource</param>
    /// <param name="a">The assembly that houses the resource, if null, uses the caller</param>
    /// <returns>The stream, else null</returns>
    public static Stream GetEmbeddedResourceAsStream(this Assembly a, string name)
    {
        ArgumentNullException.ThrowIfNull(a);
        if (null == name) return null;
        var streamNames = a.GetManifestResourceNames();
        name = name.ToLower();
        if (Array.IndexOf(streamNames, name) == -1)
        {
            foreach (var streamName in streamNames)
            {
                if (streamName.EndsWith(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    var i = name.Length + 1;
                    if (streamName.Length < i || streamName[^i] == '.')
                    {
                        name = streamName;
                        break;
                    }
                }
            }
        }
        return a.GetManifestResourceStream(name);
    }

    /// <summary>
    /// Get an embedded resource as a string
    /// </summary>
    /// <param name="name">The unqualified name of the resource</param>
    /// <param name="a">The assembly that houses the resource, if null, uses the caller</param>
    /// <returns>The string, else null</returns>
    [Obsolete("Use the async version instead")]
    public static string GetEmbeddedResourceAsString(this Assembly a, string name)
        => a.GetEmbeddedResourceAsStream(name)?.ReadToEnd();

    /// <summary>
    /// Get an embedded resource as a string
    /// </summary>
    /// <param name="name">The unqualified name of the resource</param>
    /// <param name="a">The assembly that houses the resource, if null, uses the caller</param>
    /// <returns>The string, else null</returns>
    public static Task<string> GetEmbeddedResourceAsStringAsync(this Assembly a, string name)
    {
        var st = a.GetEmbeddedResourceAsStream(name);
        return st?.ReadToEndAsync();
    }
}
