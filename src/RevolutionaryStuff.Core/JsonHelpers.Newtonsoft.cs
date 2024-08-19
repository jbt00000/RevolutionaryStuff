using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace RevolutionaryStuff.Core;

public static partial class JsonHelpers
{
    private static readonly JsonSerializerSettings FallbackSerializerSettings;
    public static JsonSerializerSettings DefaultSerializerSettings;

    static JsonHelpers()
    {
        DefaultSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        DefaultSerializerSettings.Converters.Add(new StringEnumConverter());
        FallbackSerializerSettings = DefaultSerializerSettings;
    }

    public static string GetString(this IDictionary<string, JToken> extensionData, string key, string missing = default)
        => extensionData != null && key != null && extensionData.TryGetValue(key, out var je) ? je.Value<string>() : missing;

    public static int GetInt(this IDictionary<string, JToken> extensionData, string key, int missing = default)
        => extensionData != null && key != null && extensionData.TryGetValue(key, out var je) ? je.Value<int>() : missing;

    public static string ToNewtonsoftJson(object o)
        => Services.JsonSerializers.Newtonsoft.NewtonsoftJsonSerializer.Instance.ToJson(o);

    public static T FromNewtonsoftJson<T>(string json)
        => Services.JsonSerializers.Newtonsoft.NewtonsoftJsonSerializer.Instance.FromJson<T>(json);

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
