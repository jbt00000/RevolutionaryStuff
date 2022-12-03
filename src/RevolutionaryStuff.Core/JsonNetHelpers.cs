using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace RevolutionaryStuff.Core;

public static class JsonNetHelpers
{
    private static readonly JsonSerializerSettings FallbackSerializerSettings;
    public static JsonSerializerSettings DefaultSerializerSettings;

    static JsonNetHelpers()
    {
        DefaultSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        DefaultSerializerSettings.Converters.Add(new StringEnumConverter());
        FallbackSerializerSettings = DefaultSerializerSettings;
    }

    public static string ToJson(object o)
        => JsonConvert.SerializeObject(o, DefaultSerializerSettings ?? FallbackSerializerSettings);

    public enum PathFormats
    {
        DotNotation,
        SlashNotation,
        DotOrSlashNotation,
        Default = DotOrSlashNotation
    }

    public static string[] DecomposePath(string path, PathFormats pathFormat = PathFormats.DotNotation)
    {
        return pathFormat switch
        {
            PathFormats.DotNotation => path.Split('.'),
            PathFormats.SlashNotation => path.Split('/'),
            PathFormats.DotOrSlashNotation => path.Split('.', '/'),
            _ => throw new UnexpectedSwitchValueException(pathFormat),
        };
    }

    public class PathSegment
    {
        public enum SegmentTypes
        {
            Array,
            Property,
            Object,
            ArrayIndex,
        }
        public SegmentTypes SegmentType;
        public string Name;
        public int Index;

        public static IList<PathSegment> CreateSegmentsFromJsonPath(string jsonPath, PathFormats pathFormat = PathFormats.DotNotation)
            => CreateSegmentsFromPathParts(DecomposePath(jsonPath, pathFormat));

        public static IList<PathSegment> CreateSegmentsFromPathParts(IList<string> parts)
        {
            var segments = parts.ConvertAll(part => new PathSegment { Name = part, SegmentType = SegmentTypes.Object });
            PathSegment sn = null;
            for (var z = segments.Count - 1; z >= 0; --z)
            {
                var s = segments[z];
                if (int.TryParse(s.Name, out var i))
                {
                    s.SegmentType = SegmentTypes.ArrayIndex;
                    s.Index = i;
                    s.Name = null;
                }
                else
                {
                    s.SegmentType = sn == null ? SegmentTypes.Property : sn.SegmentType == SegmentTypes.ArrayIndex ? SegmentTypes.Array : SegmentTypes.Object;
                }
                sn = s;
            }
            return segments;
        }
    }


    public static void SetValue(this JObject baseObject, string path, object val, PathFormats pathFormat = PathFormats.Default)
        => baseObject.SetValue(PathSegment.CreateSegmentsFromJsonPath(path, pathFormat), val);

    public static void SetValue(this JObject baseObject, IList<PathSegment> segments, object val)
    {
        ArgumentNullException.ThrowIfNull(baseObject);
        ArgumentNullException.ThrowIfNull(segments);

        var jval = val is JToken ? (JToken)val : new JValue(val);

        var c = (JContainer)baseObject;
        for (var z = 0; z < segments.Count; ++z)
        {
            var isLast = z == segments.Count - 1;
            var s = segments[z];
            switch (s.SegmentType)
            {
                case PathSegment.SegmentTypes.Object:
                    if (c[s.Name] == null)
                    {
                        c.Add(new JProperty(s.Name, new JObject()));
                    }
                    c = (JContainer)c[s.Name];
                    break;
                case PathSegment.SegmentTypes.Array:
                    if (c[s.Name] == null)
                    {
                        c.Add(new JProperty(s.Name, new JArray()));
                    }
                    c = (JContainer)c[s.Name];
                    break;
                case PathSegment.SegmentTypes.ArrayIndex:
                    var ja = (JArray)c;
                    for (var i = 0; i <= s.Index; ++i)
                    {
                        if (i >= ja.Count)
                        {
                            ja.Add(new JValue((string)null));
                        }
                    };
                    if (isLast)
                    {

                        ja[s.Index] = jval;
                    }
                    else if (ja.Count >= s.Index && ja[s.Index] != null && ja[s.Index].HasValues)
                    {
                        c = (JContainer)ja[s.Index];
                    }
                    else
                    {
                        ja[s.Index] = segments[z + 1].SegmentType switch
                        {
                            PathSegment.SegmentTypes.ArrayIndex => c = new JArray(),
                            _ => c = new JObject(),
                        };
                    }
                    break;
                case PathSegment.SegmentTypes.Property:
                    c[s.Name] = jval;
                    break;
                default:
                    throw new UnexpectedSwitchValueException(s.SegmentType);
            }
        }
    }
}
