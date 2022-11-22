using System.Net;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

public static class Empty
{
    public static readonly IDictionary<string, object> StringObjectDictionary = new Dictionary<string, object>().AsReadOnly();
    public static readonly Attribute[] AttributeArray = new Attribute[0];
    public static readonly byte[] ByteArray = new byte[0];
    public static readonly Guid[] GuidArray = new Guid[0];
    public static readonly int[] IntArray = new int[0];
    public static readonly long[] Int64Array = new long[0];
    public static readonly IPEndPoint IPEndPoint = new(IPAddress.None, 0);
    public static readonly object[] ObjectArray = new object[0];
    public static readonly Regex[] RegexArray = new Regex[0];
    public static readonly string[] StringArray = new string[0];
    public static readonly Type[] TypeArray = new Type[0];
    public static readonly uint[] UIntArray = new uint[0];
    public static readonly Uri[] UriArray = new Uri[0];
    public static readonly Version Version = new(0, 0, 0, 0);
}
