using System;
using System.IO;
using System.Reflection;

namespace RevolutionaryStuff.Core
{
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
            Requires.NonNull(a, nameof(a));
            if (null == name) return null;
            string[] streamNames = a.GetManifestResourceNames();
            name = name.ToLower();
            if (Array.IndexOf(streamNames, name) == -1)
            {
                foreach (string streamName in streamNames)
                {
                    if (streamName.EndsWith(name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        int i = name.Length + 1;
                        if (streamName.Length < i || streamName[streamName.Length - i] == '.')
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
        public static string GetEmbeddedResourceAsString(this Assembly a, string name)
            => a.GetEmbeddedResourceAsStream(name)?.ReadToEnd();
    }
}
