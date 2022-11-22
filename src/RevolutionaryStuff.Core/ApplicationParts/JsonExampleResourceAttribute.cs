using System.Reflection;
using Newtonsoft.Json.Linq;

namespace RevolutionaryStuff.Core.ApplicationParts;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class JsonExampleResourceAttribute : EmbeddedFileAttribute
{
    public JsonExampleResourceAttribute(string resourceName)
        : base(resourceName)
    { }

    [Obsolete]
    public JObject GetAsJObject(Type t)
        => GetAsJObject(t.Assembly);

    [Obsolete]
    public JObject GetAsJObject(Assembly a)
        => JObject.Parse(GetAsString(a));
}
