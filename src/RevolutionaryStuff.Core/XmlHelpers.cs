using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RevolutionaryStuff.Core;

internal static class XmlHelpers
{
    extension(XmlDocument source)
    {
        public XmlAttribute CreateAttributeWithValue(string name, string ns, string val, string prefix = null)
        {
            var attr = prefix == null ? source.CreateAttribute(name, ns) : source.CreateAttribute(prefix, name, ns);
            attr.Value = val;
            return attr;
        }
    }

    extension(XmlWriter source)
    {
        public void WriteElement(string localName, string ns, string value, object attrs = null)
        {
            source.WriteStartElement(localName, ns);
            if (attrs != null)
            {
                var d = TypeHelpers.ToPropertyValueDictionary(attrs);
                if (d.Count > 0)
                {
                    foreach (var kvp in d)
                    {
                        source.WriteAttributeString(kvp.Key, null, Stuff.ObjectToString(kvp.Value));
                    }
                }
            }
            if (value != null)
            {
                source.WriteString(value);
            }
            source.WriteEndElement();
        }
    }

    /*

    Starting getting this JOY when calling ser.ToXml()...

    System.InvalidOperationException
      HResult=0x80131509
      Message=Set XmlWriterSettings.Async to true if you want to use Async Methods.
      Source=System.Private.Xml
      StackTrace:
       at System.Xml.XmlEncodedRawTextWriter.CheckAsyncCall()
       at System.Xml.XmlEncodedRawTextWriter.WriteEndElementAsync(String prefix, String localName, String ns)
       at System.Xml.XmlWellFormedWriter.WriteEndElementAsync_NoAdvanceState()
       at System.Xml.XmlWellFormedWriter.<>c.<WriteEndElementAsync>b__122_0(XmlWellFormedWriter thisRef)
       at System.Xml.XmlWellFormedWriter.WriteEndElementAsync()
       at Traffk.Velogica.Models.PushServiceModels.SendResponse.<System-Xml-Serialization-IXmlSerializable-WriteXml>d__29.MoveNext() in C:\Users\JasonThomas\source\repos\traffk\HealthInformationPortal\src\Traffk.Velogica\Models\PushServiceModels.cs:line 149
       at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
       at System.Threading.Tasks.Task.<>c.<ThrowAsync>b__128_1(Object state)
       at System.Threading.QueueUserWorkItemCallback.<>c.<.cctor>b__6_0(QueueUserWorkItemCallback quwi)
       at System.Threading.ExecutionContext.RunForThreadPoolUnsafe[TState](ExecutionContext executionContext, Action`1 callback, TState& state)
       at System.Threading.QueueUserWorkItemCallback.Execute()
       at System.Threading.ThreadPoolWorkQueue.Dispatch()
       at System.Threading.PortableThreadPool.WorkerThread.WorkerThreadStart()

      This exception was originally thrown at this call stack:
        [External Code]
        Traffk.Velogica.Models.PushServiceModels.SendResponse.System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter) in PushServiceModels.cs
        [External Code]                  

     */
    extension(XmlSerializer source)
    {
        public string ToXml(object o, bool allowAsync = true)
        {
            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings() { Async = allowAsync }))
            {
                source.Serialize(xmlWriter, o);
            }
            return sb.ToString();
        }
    }
}
