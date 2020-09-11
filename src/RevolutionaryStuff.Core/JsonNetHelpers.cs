using Newtonsoft.Json.Linq;

namespace RevolutionaryStuff.Core
{
    public static class JsonNetHelpers
    {
        public enum PathFormats
        { 
            DotNotation
        }

        private static string[] DecomposePath(string path, PathFormats pathFormat)
        {
            switch (pathFormat)
            {
                case PathFormats.DotNotation:
                    return path.Split('.');
                default:
                    throw new UnexpectedSwitchValueException(pathFormat);
            }
        }

        public static void SetValue(this JObject baseObject, string path, object val, PathFormats pathFormat = PathFormats.DotNotation)
        {
            Requires.NonNull(baseObject, nameof(baseObject));
            Requires.Text(path, nameof(path));

            var c = (JContainer)baseObject;
            var parts = DecomposePath(path, pathFormat);
            for (int z = 0; z < parts.Length; ++z)
            {
                var name = parts[z];
                bool isLast = z == parts.Length - 1;
                if (c[name] == null)
                {
                    if (isLast)
                    {
                        c.Add(new JProperty(name, val));
                        return;
                    }
                    else
                    {
                        if (int.TryParse(parts[z+1], out var i))
                        {
                            c.Add(new JProperty(name, new JArray()));
                        }
                        else
                        {
                            c.Add(new JProperty(name, new JObject()));
                        }
                    }
                }
                if (isLast)
                {
                    c.Remove(p => p.Path == name);
                    c.Add(new JProperty(name, val));
                    return;
                }
                c = (JContainer)c[name];
            }
        }
    }
}
