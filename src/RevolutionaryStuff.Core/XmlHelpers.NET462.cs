using System.Xml;

namespace RevolutionaryStuff.Core;
public static partial class XmlHelpers
{
    public static XmlAttribute CreateAttributeWithValue(this XmlDocument doc, string name, string ns, string val, string prefix = null)
    {
        var attr = prefix == null ? doc.CreateAttribute(name, ns) : doc.CreateAttribute(prefix, name, ns);
        attr.Value = val;
        return attr;
    }
}
