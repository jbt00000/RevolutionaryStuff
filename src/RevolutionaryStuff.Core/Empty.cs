using System.Net;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides reusable empty collections and default values.
/// </summary>
public static class Empty
{
    /// <summary>
    /// An empty, read-only dictionary with string keys and object values.
    /// </summary>
    public static readonly IDictionary<string, object> StringObjectDictionary = new Dictionary<string, object>().AsReadOnlyDictionary();

    /// <summary>
    /// An empty array of attributes.
    /// </summary>
    public static readonly Attribute[] AttributeArray = [];

    /// <summary>
    /// An empty byte array.
    /// </summary>
    public static readonly byte[] ByteArray = [];

    /// <summary>
    /// An empty array of GUIDs.
    /// </summary>
    public static readonly Guid[] GuidArray = [];

    /// <summary>
    /// An empty array of integers.
    /// </summary>
    public static readonly int[] IntArray = [];

    /// <summary>
    /// An empty array of 64-bit integers.
    /// </summary>
    public static readonly long[] Int64Array = [];

    /// <summary>
    /// An endpoint using <see cref="IPAddress.None"/> and port zero.
    /// </summary>
    public static readonly IPEndPoint IPEndPoint = new(IPAddress.None, 0);

    /// <summary>
    /// An empty array of objects.
    /// </summary>
    public static readonly object[] ObjectArray = [];

    /// <summary>
    /// An empty array of regular expressions.
    /// </summary>
    public static readonly Regex[] RegexArray = [];

    /// <summary>
    /// An empty array of strings.
    /// </summary>
    public static readonly string[] StringArray = [];

    /// <summary>
    /// An empty array of types.
    /// </summary>
    public static readonly Type[] TypeArray = [];

    /// <summary>
    /// An empty array of unsigned integers.
    /// </summary>
    public static readonly uint[] UIntArray = [];

    /// <summary>
    /// An empty array of URIs.
    /// </summary>
    public static readonly Uri[] UriArray = [];

    /// <summary>
    /// A version with all components set to zero.
    /// </summary>
    public static readonly Version Version = new(0, 0, 0, 0);
}
