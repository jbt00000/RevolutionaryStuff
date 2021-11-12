using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core.ApplicationParts;

[AttributeUsage(AttributeTargets.Class)]
public sealed class JsonSchemaResourceAttribute : EmbeddedFileAttribute
{
    public JsonSchemaResourceAttribute(string resourceName)
        : base(resourceName)
    { }

    public JSchema GetAsJSchema(Type t)
        => GetAsJSchema(t.Assembly);

    public JSchema GetAsJSchema(Assembly a)
        => Cache.DataCacher.FindOrCreateValue(
            Cache.CreateKey(a.FullName, nameof(GetAsJSchema), ResourceName), () =>
            {
                var jsonSchema = GetAsString(a);
                return JSchema.Parse(jsonSchema);
            });
    public static void ValidateJson(Type t, string json)
    {
        Requires.NonNull(t, nameof(t));
        var attr = t.GetCustomAttribute<JsonSchemaResourceAttribute>();
        Requires.NonNull(attr, $"{nameof(JsonSchemaResourceAttribute)} missing from {t}");
        Requires.Text(json, nameof(json));
        var s = attr.GetAsJSchema(t.Assembly);
        var jo = JObject.Parse(json);
        Requires.NonNull(jo, nameof(jo));
        jo.Validate(s);
    }
}
