using System.Net;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

public static class Empty
{
    public static readonly IDictionary<string, object> StringObjectDictionary = new Dictionary<string, object>().AsReadOnlyDictionary();
    public static readonly Attribute[] AttributeArray = [];
    public static readonly byte[] ByteArray = [];
    public static readonly Guid[] GuidArray = [];
    public static readonly int[] IntArray = [];
    public static readonly long[] Int64Array = [];
    public static readonly IPEndPoint IPEndPoint = new(IPAddress.None, 0);
    public static readonly object[] ObjectArray = [];
    public static readonly Regex[] RegexArray = [];
    public static readonly string[] StringArray = [];
    public static readonly Type[] TypeArray = [];
    public static readonly uint[] UIntArray = [];
    public static readonly Uri[] UriArray = [];
    public static readonly Version Version = new(0, 0, 0, 0);
}
