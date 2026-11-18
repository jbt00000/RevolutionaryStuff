using System.IO;
using System.Reflection;

namespace RevolutionaryStuff.Core;

public static class ResourceHelpers
{
    extension(Assembly source)
    {
        /// <summary>
        /// Get an embedded resource as a stream
        /// </summary>
        /// <param name="name">The unqualified name of the resource</param>
        /// <param name="source">The assembly that houses the resource, if null, uses the caller</param>
        /// <returns>The stream, else null</returns>
        public Stream GetEmbeddedResourceAsStream(string name)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (null == name) return null;
            var streamNames = source.GetManifestResourceNames();
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
            return source.GetManifestResourceStream(name);
        }
        /// <summary>
        /// Get an embedded resource as a string
        /// </summary>
        /// <param name="name">The unqualified name of the resource</param>
        /// <param name="source">The assembly that houses the resource, if null, uses the caller</param>
        /// <returns>The string, else null</returns>
        [Obsolete("Use the async version instead")]
        public string GetEmbeddedResourceAsString(string name)
            => source.GetEmbeddedResourceAsStream(name)?.ReadToEnd();
        /// <summary>
        /// Get an embedded resource as a string
        /// </summary>
        /// <param name="name">The unqualified name of the resource</param>
        /// <param name="source">The assembly that houses the resource, if null, uses the caller</param>
        /// <returns>The string, else null</returns>
        public Task<string> GetEmbeddedResourceAsStringAsync(string name)
        {
            var st = source.GetEmbeddedResourceAsStream(name);
            return st?.ReadToEndAsync();
        }
    }




}
