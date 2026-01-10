using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides parameter validation methods for enforcing preconditions and guard clauses.
/// All methods throw exceptions when validation fails, following the fail-fast principle.
/// </summary>
public static class Requires
{
    /// <summary>
    /// Validates that a string is a valid URL.
    /// </summary>
    /// <param name="arg">The string to validate as a URL.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="arg"/> is not a valid URL.</exception>
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

    /// <summary>
    /// Validates that a value is a member of a specified set.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    /// <param name="set">The set of valid values.</param>
    /// <param name="setName">The name of the set.</param>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument.</param>
    /// <param name="nullInputOk">If <c>true</c>, allows null input without validation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="set"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is not in the set.</exception>
    public static void SetMembership<T>(ICollection<T> set, string setName, T arg, string argName, bool nullInputOk = false)
    {
        if (nullInputOk && arg == null) return;
        ArgumentNullException.ThrowIfNull(set, setName);
        if (!set.Contains(arg)) throw new ArgumentOutOfRangeException(argName, $"[{arg}] not a member of {setName}.");
    }

    /// <summary>
    /// Validates that array arguments (offset and size) are within valid bounds.
    /// </summary>
    /// <param name="arg">The array/list to validate against.</param>
    /// <param name="offset">The starting offset in the array.</param>
    /// <param name="size">The number of elements to process.</param>
    /// <param name="argName">The name of the array argument.</param>
    /// <param name="minSize">The minimum required size.</param>
    /// <param name="nullable">If <c>true</c>, allows null arrays.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null and <paramref name="nullable"/> is false.</exception>
    /// <exception cref="ArgumentException">Thrown when offset or size are invalid.</exception>
    public static void ArrayArg(IList arg, int offset, int size, string argName, int minSize = 0, bool nullable = false)
    {
        if (!nullable) ArgumentNullException.ThrowIfNull(arg, argName);
        if (size < minSize) throw new ArgumentException($"size must be >= {minSize}");
        if (offset < 0) throw new ArgumentException("offset must be >= 0");
        if (size + offset > arg.Count)
            throw new ArgumentException($"size+offset must be <= {argName}.Count");
    }

    /// <summary>
    /// Validates that a list is not null and meets minimum size requirements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="arg">The list to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <param name="minSize">The minimum required number of elements.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the list has fewer than <paramref name="minSize"/> elements.</exception>
    public static void ListArg<T>(IList<T> arg, [CallerArgumentExpression("arg")] string argName = null, int minSize = 0)
    {
        ArgumentNullException.ThrowIfNull(arg, argName);
        if (arg.Count < minSize) throw new ArgumentOutOfRangeException(argName, $"Length must be >= {minSize}");
    }

    /// <summary>
    /// Validates an object by calling its <see cref="IValidate.Validate"/> method.
    /// </summary>
    /// <param name="arg">The object to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <param name="canBeNull">If <c>true</c>, allows null values.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null and <paramref name="canBeNull"/> is false.</exception>
    /// <exception cref="Exception">Any exception thrown by the object's Validate method.</exception>
    public static void Valid(IValidate arg, [CallerArgumentExpression("arg")] string argName = null, bool canBeNull = false)
    {
        if (arg == null && canBeNull) return;
        ArgumentNullException.ThrowIfNull(arg, argName);
        arg.Validate();
    }

    /// <summary>
    /// Ensures a method or code block is called only once.
    /// </summary>
    /// <param name="alreadyCalled">A flag that tracks whether the call has already been made.</param>
    /// <exception cref="SingleCallException">Thrown when the method is called more than once.</exception>
    public static void SingleCall(ref bool alreadyCalled)
    {
        if (alreadyCalled) throw new SingleCallException();
        alreadyCalled = true;
    }

    /// <summary>
    /// Validates that a boolean condition is true.
    /// </summary>
    /// <param name="arg">The boolean value to validate.</param>
    /// <param name="argName">The name of the argument.</param>
    /// <param name="message">The error message to use if validation fails.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is false.</exception>
    public static void True(bool arg, string argName, string message)
    {
        if (arg == true) return;
        throw new ArgumentOutOfRangeException(argName, message);
    }

    /// <summary>
    /// Validates that a string is a valid file extension (starts with a period).
    /// </summary>
    /// <param name="arg">The string to validate as a file extension.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="arg"/> is not a valid extension format.</exception>
    public static void FileExtension(string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Text(arg, argName);
        if (Path.GetExtension(arg) != arg)
        {
            throw new ArgumentException("Not a valid extension", argName);
        }
    }

    /// <summary>
    /// Validates that a file exists at the specified path.
    /// </summary>
    /// <param name="arg">The file path to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the file does not exist.</exception>
    public static void FileExists(string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Text(arg, argName);
        if (!File.Exists(arg))
            throw new ArgumentException($"File=[{arg}] does not exist", argName);
    }

    /// <summary>
    /// Validates that a directory exists at the specified path.
    /// </summary>
    /// <param name="arg">The directory path to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the directory does not exist.</exception>
    public static void DirectoryExists(string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Text(arg, argName);
        if (!Directory.Exists(arg))
            throw new ArgumentException($"Directory=[{arg}] does not exist", argName);
    }

    /// <summary>
    /// Validates that an enumerable contains at least one element.
    /// </summary>
    /// <param name="arg">The enumerable to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the enumerable is empty.</exception>
    public static void HasData(IEnumerable arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        ArgumentNullException.ThrowIfNull(arg, argName);
        var e = arg.GetEnumerator();
        if (!e.MoveNext())
        {
            throw new ArgumentOutOfRangeException(argName, "This enumerable does not have any values");
        }
    }

    /// <summary>
    /// Validates that an enumerable is empty or null.
    /// </summary>
    /// <param name="arg">The enumerable to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the enumerable contains any elements.</exception>
    public static void HasNoData(IEnumerable arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg == null) return;
        var e = arg.GetEnumerator();
        if (e.MoveNext())
        {
            throw new ArgumentOutOfRangeException(argName, "This enumerable has values but must not");
        }
    }

    /// <summary>
    /// Validates that a boolean value is false.
    /// </summary>
    /// <param name="arg">The boolean value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is true.</exception>
    public static void False(bool arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg) throw new ArgumentOutOfRangeException(argName, "Must be false");
    }

    /// <summary>
    /// Validates that a boolean value is true.
    /// </summary>
    /// <param name="arg">The boolean value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is false.</exception>
    public static void True(bool arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (!arg) throw new ArgumentOutOfRangeException(argName, "Must be true");
    }

    /// <summary>
    /// Validates that two values are equal.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="arg1Name">The name of the expected argument (automatically captured).</param>
    /// <param name="arg2Name">The name of the actual argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the values are not equal.</exception>
    public static void AreEqual<T>(T expected, T actual, [CallerArgumentExpression(nameof(expected))] string arg1Name = null, [CallerArgumentExpression(nameof(actual))] string arg2Name = null)
    {
        if (!object.Equals(expected, actual))
        {
            throw new ArgumentOutOfRangeException(arg1Name, $"[{expected}] != [{actual}]");
        }
    }

    /// <summary>
    /// Validates that a value is null.
    /// </summary>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is not null.</exception>
    public static void Null(object arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (null != arg) throw new ArgumentOutOfRangeException(argName, "Must be null");
    }

    /// <summary>
    /// Validates that exactly one of the provided values is non-null.
    /// </summary>
    /// <param name="items">The values to check.</param>
    /// <exception cref="ArgumentException">Thrown when zero or more than one value is non-null.</exception>
    public static void ExactlyOneNonNull(params object[] items)
    {
        var cntNotNull = 0;
        foreach (var i in items)
        {
            cntNotNull += i == null ? 0 : 1;
        }
        if (cntNotNull != 1) throw new ArgumentException($"EXACTLY 1 of the {items.Length} MUST be non-null. {cntNotNull} were not null");
    }

    /// <summary>
    /// Validates that a type is assignable to another type.
    /// </summary>
    /// <param name="testType">The type to test.</param>
    /// <param name="isType">The type to test against.</param>
    /// <exception cref="ArgumentNullException">Thrown when either parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="testType"/> is not assignable to <paramref name="isType"/>.</exception>
    public static void IsType(Type testType, Type isType)
    {
        ArgumentNullException.ThrowIfNull(testType, nameof(testType));
        ArgumentNullException.ThrowIfNull(isType, nameof(isType));
        if (isType.GetTypeInfo().IsAssignableFrom(testType)) return;
        throw new ArgumentException($"{testType} is not a {isType}");
    }

    /// <summary>
    /// Validates that a numeric value is exactly zero.
    /// </summary>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is not zero.</exception>
    public static void Zero(double arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg != 0) throw new ArgumentOutOfRangeException(argName, "Must be = 0");
    }

    /// <summary>
    /// Validates that a numeric value is greater than or equal to zero.
    /// </summary>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is negative.</exception>
    public static void NonNegative(double arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg < 0) throw new ArgumentOutOfRangeException(argName, "Must be >= 0");
    }

    /// <summary>
    /// Validates that a numeric value is greater than or equal to zero.
    /// </summary>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is negative.</exception>
    public static void NonNegative(long arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg < 0) throw new ArgumentOutOfRangeException(argName, "Must be >= 0");
    }

    /// <summary>
    /// Validates that a numeric value is less than or equal to zero.
    /// </summary>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is positive.</exception>
    public static void NonPositive(double arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg > 0) throw new ArgumentOutOfRangeException(argName, "Must be <= 0");
    }

    /// <summary>
    /// Validates that a numeric value is less than or equal to zero.
    /// </summary>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is positive.</exception>
    public static void NonPositive(long arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg > 0) throw new ArgumentOutOfRangeException(argName, "Must be <= 0");
    }

    /// <summary>
    /// Validates that a numeric value is greater than zero.
    /// </summary>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is zero or negative.</exception>
    public static void Positive(double arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg <= 0) throw new ArgumentOutOfRangeException(argName, "Must be > 0");
    }

    /// <summary>
    /// Validates that a numeric value is greater than zero.
    /// </summary>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is zero or negative.</exception>
    public static void Positive(long arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg <= 0) throw new ArgumentOutOfRangeException(argName, "Must be > 0");
    }

    /// <summary>
    /// Validates that a numeric value is less than zero.
    /// </summary>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is zero or positive.</exception>
    public static void Negative(double arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg >= 0) throw new ArgumentOutOfRangeException(argName, "Must be < 0");
    }

    /// <summary>
    /// Validates that a numeric value is less than zero.
    /// </summary>
    /// <param name="arg">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is zero or positive.</exception>
    public static void Negative(long arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg >= 0) throw new ArgumentOutOfRangeException(argName, "Must be < 0");
    }

    /// <summary>
    /// Validates that a string matches a regular expression pattern.
    /// </summary>
    /// <param name="r">The regular expression to match against.</param>
    /// <param name="arg">The string to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> does not match the pattern.</exception>
    public static void Match(Regex r, string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Text(arg, argName, false, 0);
        if (!r.IsMatch(arg))
        {
            throw new ArgumentOutOfRangeException(argName, "does not match pattern");
        }
    }

    /// <summary>
    /// Validates that a string meets length requirements.
    /// </summary>
    /// <param name="arg">The string to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <param name="allowNull">If <c>true</c>, allows null values.</param>
    /// <param name="minLen">The minimum required length. Defaults to 1.</param>
    /// <param name="maxLen">The maximum allowed length. Defaults to int.MaxValue.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null and <paramref name="allowNull"/> is false.</exception>
    /// <exception cref="ArgumentException">Thrown when the string length is outside the specified range or when minLen > maxLen.</exception>
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

    /// <summary>
    /// Validates that a string is a valid email address.
    /// </summary>
    /// <param name="arg">The string to validate as an email address.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arg"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="arg"/> is not a valid email address format.</exception>
    public static void EmailAddress(string arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        Match(RegexHelpers.Common.EmailAddress(), arg, argName);
    }

    /// <summary>
    /// Validates stream capabilities.
    /// </summary>
    /// <param name="stream">The stream to validate.</param>
    /// <param name="argName">The name of the argument.</param>
    /// <param name="mustBeReadable">If <c>true</c>, requires the stream to be readable.</param>
    /// <param name="mustBeWriteable">If <c>true</c>, requires the stream to be writeable.</param>
    /// <param name="mustBeSeekable">If <c>true</c>, requires the stream to be seekable.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the stream does not support required operations.</exception>
    public static void StreamArg(Stream stream, string argName, bool mustBeReadable, bool mustBeWriteable,
                                 bool mustBeSeekable)
    {
        ArgumentNullException.ThrowIfNull(stream, argName);
        if (mustBeReadable && !stream.CanRead) throw new ArgumentException("Cannot read from this stream", argName);
        if (mustBeWriteable && !stream.CanWrite)
            throw new ArgumentException("Cannot write to this stream", argName);
        if (mustBeSeekable && !stream.CanSeek) throw new ArgumentException("Cannot seek in this stream", argName);
    }

    /// <summary>
    /// Validates that a stream is writeable.
    /// </summary>
    /// <param name="stream">The stream to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the stream is not writeable.</exception>
    public static void WriteableStreamArg(Stream stream, [CallerArgumentExpression("stream")] string argName = null)
    {
        StreamArg(stream, argName, false, true, false);
    }

    /// <summary>
    /// Validates that a stream is writeable and optionally seekable.
    /// </summary>
    /// <param name="stream">The stream to validate.</param>
    /// <param name="argName">The name of the argument.</param>
    /// <param name="mustBeSeekable">If <c>true</c>, also requires the stream to be seekable.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the stream does not meet the requirements.</exception>
    public static void WriteableStreamArg(Stream stream, string argName, bool mustBeSeekable)
    {
        StreamArg(stream, argName, false, true, mustBeSeekable);
    }

    /// <summary>
    /// Validates that a stream is readable.
    /// </summary>
    /// <param name="stream">The stream to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the stream is not readable.</exception>
    public static void ReadableStreamArg(Stream stream, [CallerArgumentExpression("stream")] string argName = null)
    {
        StreamArg(stream, argName, true, false, false);
    }

    /// <summary>
    /// Validates that a stream is readable and optionally seekable.
    /// </summary>
    /// <param name="stream">The stream to validate.</param>
    /// <param name="argName">The name of the argument.</param>
    /// <param name="mustBeSeekable">If <c>true</c>, also requires the stream to be seekable.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the stream does not meet the requirements.</exception>
    public static void ReadableStreamArg(Stream stream, string argName, bool mustBeSeekable)
    {
        StreamArg(stream, argName, true, false, mustBeSeekable);
    }

    /// <summary>
    /// Validates that a value is within a specified range.
    /// </summary>
    /// <param name="val">The value to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <param name="minLength">The minimum allowed value. If null, uses long.MinValue.</param>
    /// <param name="maxLength">The maximum allowed value. If null, uses long.MaxValue.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="val"/> is outside the specified range.</exception>
    public static void Between(long val, [CallerArgumentExpression("val")] string argName = null, long? minLength = null, long? maxLength = null)
    {
        if (val < minLength.GetValueOrDefault(long.MinValue)) throw new ArgumentOutOfRangeException(argName, $"must be >= {minLength}");
        if (val > maxLength.GetValueOrDefault(long.MaxValue)) throw new ArgumentOutOfRangeException(argName, $"must be <= {maxLength}");
    }

    /// <summary>
    /// Validates that a byte array meets size requirements.
    /// </summary>
    /// <param name="buf">The byte array to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <param name="minLength">The minimum required buffer length. Defaults to 0.</param>
    /// <param name="maxLength">The maximum allowed buffer length. Defaults to int.MaxValue.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="buf"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the buffer length is outside the specified range.</exception>
    public static void Buffer(byte[] buf, [CallerArgumentExpression("buf")] string argName = null, long? minLength = null, long? maxLength = null)
    {
        ArgumentNullException.ThrowIfNull(buf, argName);
        Between(buf.Length, $"{argName}.Length", minLength.GetValueOrDefault(0), maxLength.GetValueOrDefault(int.MaxValue));
    }

    /// <summary>
    /// Validates that a port number is within the valid range (1-65536 or 0-65536 if zero is allowed).
    /// </summary>
    /// <param name="portNumber">The port number to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <param name="allowZero">If <c>true</c>, allows port number 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="portNumber"/> is outside the valid range.</exception>
    public static void PortNumber(int portNumber, [CallerArgumentExpression("portNumber")] string argName = null, bool allowZero = false)
        => Between(portNumber, argName, allowZero ? 0 : 1, 65536);

    /// <summary>
    /// Validates that a DataTable has zero rows.
    /// </summary>
    /// <param name="dt">The DataTable to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dt"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the table already contains rows.</exception>
    public static void ZeroRows(DataTable dt, [CallerArgumentExpression("dt")] string argName = null)
    {
        ArgumentNullException.ThrowIfNull(dt, argName ?? nameof(dt));
        if (dt.Rows.Count > 0) throw new ArgumentException("dt must not already have any rows", nameof(dt));
    }

    /// <summary>
    /// Validates that a DataTable has zero columns.
    /// </summary>
    /// <param name="dt">The DataTable to validate.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dt"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the table already contains columns.</exception>
    public static void ZeroColumns(DataTable dt, [CallerArgumentExpression("dt")] string argName = null)
    {
        ArgumentNullException.ThrowIfNull(dt, argName ?? nameof(dt));
        if (dt.Columns.Count > 0) throw new ArgumentException("dt must not already have any columns", nameof(dt));
    }

    /// <summary>
    /// Validates that a string contains valid XML.
    /// </summary>
    /// <param name="xml">The string to validate as XML.</param>
    /// <param name="argName">The name of the argument (automatically captured).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="xml"/> is null or empty.</exception>
    /// <exception cref="System.Xml.XmlException">Thrown when the string is not valid XML.</exception>
    public static void Xml(string xml, [CallerArgumentExpression("xml")] string argName = null)
    {
        Text(xml, argName);
        XDocument.Parse(xml);
    }
}
