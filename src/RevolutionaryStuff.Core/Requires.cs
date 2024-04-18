using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core;

public static class Requires
{
    public static void Url(string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Text(arg, argName);
        try
        {
            new Uri(arg);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"{argName} must be a valid url", ex);
        }
    }

    public static void SetMembership<T>(ICollection<T> set, string setName, T arg, string argName, bool nullInputOk = false)
    {
        if (nullInputOk && arg == null) return;
        ArgumentNullException.ThrowIfNull(set, setName);
        if (!set.Contains(arg)) throw new ArgumentOutOfRangeException(argName, $"[{arg}] not a member of {setName}.");
    }

    public static void ArrayArg(IList arg, int offset, int size, string argName, int minSize = 0, bool nullable = false)
    {
        if (!nullable) ArgumentNullException.ThrowIfNull(arg, argName);
        if (size < minSize) throw new ArgumentException($"size must be >= {minSize}");
        if (offset < 0) throw new ArgumentException("offset must be >= 0");
        if (size + offset > arg.Count)
            throw new ArgumentException($"size+offset must be <= {argName}.Count");
    }

    public static void ListArg<T>(IList<T> arg, [CallerArgumentExpression("arg")] string argName = null, int minSize = 0)
    {
        ArgumentNullException.ThrowIfNull(arg, argName);
        if (arg.Count < minSize) throw new ArgumentOutOfRangeException(argName, $"Length must be >0 {minSize}");
    }

    public static void Valid(IValidate arg, [CallerArgumentExpression("arg")] string argName = null, bool canBeNull = false)
    {
        if (arg == null && canBeNull) return;
        ArgumentNullException.ThrowIfNull(arg, argName);
        arg.Validate();
    }

    public static void SingleCall(ref bool alreadyCalled)
    {
        if (alreadyCalled) throw new SingleCallException();
        alreadyCalled = true;
    }

    public static void True(bool arg, string argName, string message)
    {
        if (arg == true) return;
        throw new ArgumentOutOfRangeException(argName, message);
    }

    public static void FileExtension(string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Text(arg, argName);
        if (Path.GetExtension(arg) != arg)
        {
            throw new ArgumentException("Not a valid extension", argName);
        }
    }

    public static void FileExists(string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Text(arg, argName);
        if (!File.Exists(arg))
            throw new ArgumentException($"File=[{arg}] does not exist", argName);
    }

    public static void DirectoryExists(string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Text(arg, argName);
        if (!Directory.Exists(arg))
            throw new ArgumentException($"Directory=[{arg}] does not exist", argName);
    }

    public static void HasData(IEnumerable arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        ArgumentNullException.ThrowIfNull(arg, argName);
        var e = arg.GetEnumerator();
        if (!e.MoveNext())
        {
            throw new ArgumentOutOfRangeException(argName, "This enumerable does not have any values");
        }
    }

    public static void HasNoData(IEnumerable arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg == null) return;
        var e = arg.GetEnumerator();
        if (e.MoveNext())
        {
            throw new ArgumentOutOfRangeException(argName, "This enumerable has values but must not");
        }
    }

    public static void False(bool arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg) throw new ArgumentOutOfRangeException(argName, "Must be false");
    }

    public static void True(bool arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (!arg) throw new ArgumentOutOfRangeException(argName, "Must be true");
    }

    public static void Null(object arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (null != arg) throw new ArgumentOutOfRangeException(argName, "Must be null");
    }

    public static void ExactlyOneNonNull(params object[] items)
    {
        var cntNotNull = 0;
        foreach (var i in items)
        {
            cntNotNull += i == null ? 0 : 1;
        }
        if (cntNotNull != 1) throw new ArgumentException($"EXACTLY 1 of the {items.Length} MUST be non-null. {cntNotNull} were not null");
    }

    public static void IsType(Type testType, Type isType)
    {
        ArgumentNullException.ThrowIfNull(testType, nameof(testType));
        ArgumentNullException.ThrowIfNull(isType, nameof(isType));
        if (isType.GetTypeInfo().IsAssignableFrom(testType)) return;
        throw new ArgumentException($"{testType} is not a {isType}");
    }

    public static void Zero(double arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg != 0) throw new ArgumentOutOfRangeException(argName, "Must be = 0");
    }

    public static void NonNegative(double arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg < 0) throw new ArgumentOutOfRangeException(argName, "Must be >= 0");
    }

    public static void NonNegative(long arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg < 0) throw new ArgumentOutOfRangeException(argName, "Must be >= 0");
    }

    public static void NonPositive(double arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg > 0) throw new ArgumentOutOfRangeException(argName, "Must be <= 0");
    }

    public static void NonPositive(long arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg > 0) throw new ArgumentOutOfRangeException(argName, "Must be <= 0");
    }

    public static void Positive(double arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg <= 0) throw new ArgumentOutOfRangeException(argName, "Must be > 0");
    }

    public static void Positive(long arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg <= 0) throw new ArgumentOutOfRangeException(argName, "Must be > 0");
    }

    public static void Negative(double arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg >= 0) throw new ArgumentOutOfRangeException(argName, "Must be < 0");
    }

    public static void Negative(long arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg >= 0) throw new ArgumentOutOfRangeException(argName, "Must be < 0");
    }

    public static void Match(Regex r, string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Text(arg, argName, false, 0);
        if (!r.IsMatch(arg))
        {
            throw new ArgumentOutOfRangeException(argName, "does not match pattern");
        }
    }

    public static void Text(string arg, [CallerArgumentExpression("arg")] string argName = null, bool allowNull = false, int minLen = 1, int maxLen = int.MaxValue)
    {
        if (minLen > maxLen) throw new ArgumentException("minLen cannot be > than maxLen");
        if (!allowNull && null == arg) throw new ArgumentNullException(argName);
        arg ??= "";
        if (arg.Length < minLen)
        {
            throw new ArgumentException($"{argName} must be >= {minLen} chars", argName);
        }
        if (maxLen > -1 && arg.Length > maxLen)
        {
            throw new ArgumentException($"{argName} must be <= {maxLen} chars", argName);
        }
    }

    public static void EmailAddress(string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Match(RegexHelpers.Common.EmailAddress, arg, argName);
    }

    public static void StreamArg(Stream stream, string argName, bool mustBeReadable, bool mustBeWriteable,
                                 bool mustBeSeekable)
    {
        ArgumentNullException.ThrowIfNull(stream, argName);
        if (mustBeReadable && !stream.CanRead) throw new ArgumentException("Cannot read from this stream", argName);
        if (mustBeWriteable && !stream.CanWrite)
            throw new ArgumentException("Cannot write to this stream", argName);
        if (mustBeSeekable && !stream.CanSeek) throw new ArgumentException("Cannot seek in this stream", argName);
    }

    public static void WriteableStreamArg(Stream stream, [CallerArgumentExpression("stream")] string argName = null)
    {
        StreamArg(stream, argName, false, true, false);
    }

    public static void WriteableStreamArg(Stream stream, string argName, bool mustBeSeekable)
    {
        StreamArg(stream, argName, false, true, mustBeSeekable);
    }

    public static void ReadableStreamArg(Stream stream, [CallerArgumentExpression("stream")] string argName = null)
    {
        StreamArg(stream, argName, true, false, false);
    }

    public static void ReadableStreamArg(Stream stream, string argName, bool mustBeSeekable)
    {
        StreamArg(stream, argName, true, false, mustBeSeekable);
    }

    public static void Between(long val, [CallerArgumentExpression("val")] string argName = null, long? minLength = null, long? maxLength = null)
    {
        if (val < minLength.GetValueOrDefault(long.MinValue)) throw new ArgumentOutOfRangeException(argName, $"must be >= {minLength}");
        if (val > maxLength.GetValueOrDefault(long.MaxValue)) throw new ArgumentOutOfRangeException(argName, $"must be <= {maxLength}");
    }

    public static void Buffer(byte[] buf, [CallerArgumentExpression("buf")] string argName = null, long? minLength = null, long? maxLength = null)
    {
        ArgumentNullException.ThrowIfNull(buf, argName);
        Between(buf.Length, $"{argName}.Length", minLength.GetValueOrDefault(0), maxLength.GetValueOrDefault(int.MaxValue));
    }

    public static void PortNumber(int portNumber, [CallerArgumentExpression("portNumber")] string argName = null, bool allowZero = false)
        => Between(portNumber, argName, allowZero ? 0 : 1, 65536);

    public static void ZeroRows(DataTable dt, [CallerArgumentExpression("dt")] string argName = null)
    {
        ArgumentNullException.ThrowIfNull(dt, argName ?? nameof(dt));
        if (dt.Rows.Count > 0) throw new ArgumentException("dt must not already have any rows", nameof(dt));
    }

    public static void ZeroColumns(DataTable dt, [CallerArgumentExpression("dt")] string argName = null)
    {
        ArgumentNullException.ThrowIfNull(dt, argName ?? nameof(dt));
        if (dt.Columns.Count > 0) throw new ArgumentException("dt must not already have any columns", nameof(dt));
    }

    public static void Xml(string xml, [CallerArgumentExpression("xml")] string argName = null)
    {
        Text(xml, argName);
        XDocument.Parse(xml);
    }
}
