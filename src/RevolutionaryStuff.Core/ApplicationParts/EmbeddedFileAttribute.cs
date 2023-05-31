using System.Reflection;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core.ApplicationParts;

public abstract class EmbeddedFileAttribute : Attribute
{
    public string ResourceName;

    public EmbeddedFileAttribute(string resourceName)
    {
        ResourceName = resourceName;
    }

    public string GetAsString(Type t)
        => GetAsString(t.Assembly);

    public string GetAsString(Assembly a)
        => PermaCache.FindOrCreate(a.FullName, nameof(GetAsString), ResourceName, () => ResourceHelpers.GetEmbeddedResourceAsString(a, ResourceName));
}

