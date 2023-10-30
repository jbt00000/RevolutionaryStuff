using System.Net;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

public static class Empty
{
    public static readonly IDictionary<string, object> StringObjectDictionary = new Dictionary<string, object>().AsReadOnlyDictionary();
    public static readonly Attribute[] AttributeArray = Array.Empty<Attribute>();
    public static readonly byte[] ByteArray = Array.Empty<byte>();
    public static readonly Guid[] GuidArray = Array.Empty<Guid>();
    public static readonly int[] IntArray = Array.Empty<int>();
    public static readonly long[] Int64Array = Array.Empty<long>();
    public static readonly IPEndPoint IPEndPoint = new(IPAddress.None, 0);
    public static readonly object[] ObjectArray = Array.Empty<object>();
    public static readonly Regex[] RegexArray = Array.Empty<Regex>();
    public static readonly string[] StringArray = Array.Empty<string>();
    public static readonly Type[] TypeArray = Array.Empty<Type>();
    public static readonly uint[] UIntArray = Array.Empty<uint>();
    public static readonly Uri[] UriArray = Array.Empty<Uri>();
    public static readonly Version Version = new(0, 0, 0, 0);
}
