using System.Xml;

namespace RevolutionaryStuff.Core
{
    public static partial class XmlHelpers
    {
        public static void WriteElement(this XmlWriter writer, string localName, string ns, string value, object attrs=null)
        {
            writer.WriteStartElement(localName, ns);
            if (attrs != null)
            {
                var d = TypeHelpers.ToPropertyValueDictionary(attrs);
                if (d.Count > 0)
                {
                    foreach (var kvp in d)
                    {
                        writer.WriteAttributeString(kvp.Key, null, Stuff.ObjectToString(kvp.Value));
                    }
                }
            }
            if (value != null)
            {
                writer.WriteString(value);
            }
            writer.WriteEndElement();
        }
    }
}
