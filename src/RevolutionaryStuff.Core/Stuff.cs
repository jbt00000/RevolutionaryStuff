using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides general utility methods and application-level constants.
/// Includes random number generation, object manipulation, file cleanup, and application metadata.
/// </summary>
public static class Stuff
{
    /// <summary>
    /// The assembly containing this class (RevolutionaryStuff.Core).
    /// </summary>
    public static readonly Assembly ThisAssembly;

    /// <summary>
    /// A default logger used primarily by static functions.  
    /// Setting this to null will instead set it to NullLogger.Instance.
    /// </summary>
    public static ILogger LoggerOfLastResort
    {
        get;
        set => field = value ?? NullLogger.Instance;
    } = NullLogger.Instance;

    static Stuff()
    {
        ThisAssembly = typeof(Stuff).GetTypeInfo().Assembly;
        var a = Assembly.GetEntryAssembly();
        var info = a?.GetInfo();
        ApplicationName = StringHelpers.Coalesce(info?.Title, a?.GetName().Name, "Unnamed");
        ApplicationFamily = StringHelpers.Coalesce(info?.Product, info?.Company, ApplicationName);
    }

    /// <summary>
    /// A polite way to express frustration (from Q*bert video game).
    /// Value: "@!#?@!"
    /// </summary>
    public const string Qbert = "@!#?@!";

    /// <summary>
    /// Base URN for RevolutionaryStuff components.
    /// Value: "urn:www.revolutionarystuff.com"
    /// </summary>
    public const string BaseRsllcUrn = "urn:www.revolutionarystuff.com";

    /// <summary>
    /// Prefix for configuration section names.
    /// Value: "Rsllc"
    /// </summary>
    internal const string ConfigSectionNamePrefix = "Rsllc";

    /// <summary>
    /// The name of the running application, determined from assembly metadata.
    /// </summary>
    public static readonly string ApplicationName;

    /// <summary>
    /// The family or product name of the running application, determined from assembly metadata.
    /// </summary>
    public static readonly string ApplicationFamily;

    /// <summary>
    /// The UTC timestamp when the application started.
    /// </summary>
    public static readonly DateTimeOffset ApplicationStartedAt = DateTimeOffset.UtcNow;

    /// <summary>
    /// A unique identifier for this application instance.
    /// Generated once per application run.
    /// </summary>
    public static readonly Guid ApplicationInstanceId = Guid.NewGuid();

    /// <summary>
    /// Random number generator with a fixed seed (19740409).
    /// Useful for testing scenarios requiring reproducible random sequences.
    /// </summary>
    public static readonly Random RandomWithFixedSeed = new(19740409);

    /// <summary>
    /// Random number generator with a random seed value.
    /// </summary>
    public static readonly Random RandomWithRandomSeed = new(Crypto.Salt.RandomInteger);

    /// <summary>
    /// Default instance of a random number generator.
    /// Uses a random seed (same as <see cref="RandomWithRandomSeed"/>).
    /// </summary>
    public static readonly Random Random = RandomWithRandomSeed;

    /// <summary>
    /// Does nothing. Used as a line where breakpoints can be set during debugging.
    /// This method is compiled out in Release builds (Conditional("DEBUG")).
    /// </summary>
    /// <param name="args">Optional parameters to prevent them from being compiled out.</param>
    [Conditional("DEBUG")]
    public static void NoOp(params object[] args)
    { }

    /// <summary>
    /// Safely converts an object to its string representation.
    /// </summary>
    /// <param name="o">The object to convert.</param>
    /// <returns>The string representation of the object, or null if the object is null.</returns>
    public static string ToString(object o)
        => o?.ToString();

    /// <summary>
    /// Swaps the values of two variables.
    /// </summary>
    /// <typeparam name="T">The type of the variables.</typeparam>
    /// <param name="a">The first variable.</param>
    /// <param name="b">The second variable.</param>
    public static void Swap<T>(ref T a, ref T b)
        => (b, a) = (a, b);

    /// <summary>
    /// Returns the minimum of two comparable values.
    /// </summary>
    /// <typeparam name="T">The type of values to compare (must implement IComparable&lt;T&gt;).</typeparam>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The smaller of the two values.</returns>
    public static T Min<T>(T a, T b) where T : IComparable<T>
        => a.CompareTo(b) < 0 ? a : b;

    /// <summary>
    /// Returns the maximum of two comparable values.
    /// </summary>
    /// <typeparam name="T">The type of values to compare (must implement IComparable&lt;T&gt;).</typeparam>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The larger of the two values.</returns>
    public static T Max<T>(T a, T b) where T : IComparable<T>
        => a.CompareTo(b) < 0 ? b : a;

    /// <summary>
    /// Converts a Windows tick count to a DateTime.
    /// The tick count must have been created in the current Windows session.
    /// </summary>
    /// <param name="tickCount">The tick count from Environment.TickCount.</param>
    /// <returns>The approximate DateTime when the tick count was recorded.</returns>
    public static DateTime TickCount2DateTime(int tickCount)
    {
        var n = DateTime.Now;
        var tc = Environment.TickCount;
        return n.AddMilliseconds(tickCount - tc);
    }

    /// <summary>
    /// Gets all values defined in an enumeration type.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <returns>A collection of all enumeration values.</returns>
    public static IEnumerable<T> GetEnumValues<T>()
    {
        var ret = new List<T>();
        foreach (var v in Enum.GetValues(typeof(T)))
        {
            ret.Add((T)v);
        }
        return ret;
    }

    /// <summary>
    /// Tests whether a flags enumeration has a specific flag set.
    /// </summary>
    /// <param name="Flags">The flags enumeration value.</param>
    /// <param name="Val">The flag value to test for.</param>
    /// <returns><c>true</c> if the flag is set; otherwise, <c>false</c>.</returns>
    public static bool FlagEq(Enum Flags, Enum Val)
    {
        var flags = (long)Convert.ChangeType(Flags, typeof(long), null);
        var val = (long)Convert.ChangeType(Val, typeof(long), null);
        return val == (flags & val);
    }

    /// <summary>
    /// Disposes objects that implement IDisposable.
    /// Safely handles null values and exceptions during disposal.
    /// </summary>
    /// <param name="os">The objects to dispose.</param>
    public static void Dispose(params object[] os)
    {
        if (os == null) return;
        foreach (var o in os)
        {
            if (o is not IDisposable d) return;
            try
            {
                d.Dispose();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }

    /// <summary>
    /// Converts an object to its string representation with an optional fallback.
    /// </summary>
    /// <param name="o">The object to convert.</param>
    /// <param name="fallback">The fallback value if the object is null.</param>
    /// <returns>The string representation or the fallback value.</returns>
    internal static string ObjectToString(object o, string fallback = null)
        => o?.ToString() ?? fallback;

    private static readonly IList<string> FilesToDeleteOnExit = [];

    /// <summary>
    /// Marks a file for cleanup when the application exits.
    /// The file will be deleted during the <see cref="Cleanup"/> call.
    /// </summary>
    /// <param name="filePath">The path of the file to mark for cleanup.</param>
    /// <param name="setAttributesAsTempFile">
    /// If <c>true</c>, sets the file's attributes to temporary.
    /// </param>
    public static void MarkFileForCleanup(string filePath, bool setAttributesAsTempFile = true)
    {
        if (filePath == null) return;
        if (setAttributesAsTempFile)
        {
            try
            {
                File.SetAttributes(filePath, FileAttributes.Temporary);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
        lock (FilesToDeleteOnExit)
        {
            FilesToDeleteOnExit.Add(filePath);
        }
    }

    /// <summary>
    /// Deletes all files that have been marked for cleanup via <see cref="MarkFileForCleanup"/>.
    /// Should be called during application shutdown.
    /// </summary>
    public static void Cleanup()
    {
        lock (FilesToDeleteOnExit)
        {
            foreach (var fn in FilesToDeleteOnExit)
            {
                FileSystemHelpers.FileTryDelete(fn);
            }
        }
    }

    /// <summary>
    /// Creates a random code string suitable for CAPTCHA or verification codes.
    /// </summary>
    /// <param name="numChars">The number of characters in the code. Defaults to 6.</param>
    /// <returns>A random alphanumeric code string.</returns>
    public static string CreateRandomCode(int numChars = 6)
        => Services.CodeStringGenerator.DefaultCodeStringGenerator.Instance.CreateCaptchaCharactersCode(numChars);

    /// <summary>
    /// Converts a serialized JSON property path to a .NET property path.
    /// Maps JSON property names to their corresponding .NET property names.
    /// </summary>
    /// <param name="t">The type containing the properties.</param>
    /// <param name="serializedPath">The JSON property path (e.g., "firstName.middleName").</param>
    /// <returns>The .NET property path (e.g., "FirstName.MiddleName"), or null if not found.</returns>
    public static string GetPathFromSerializedPath(Type t, string serializedPath)
        => PermaCache.FindOrCreate(
            t, serializedPath,
            () =>
            {
                if (serializedPath == null) return null;
                var left = serializedPath.LeftOf(".");
                var right = serializedPath.RightOf(".").TrimOrNull();

                foreach (var pi in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (pi.HasJsonIgnoreAttribute()) continue;
                    var jpn = pi.GetJsonPropertyName();
                    if (jpn == left)
                    {
                        left = pi.Name;
                        if (right == null) return left;
                        right = GetPathFromSerializedPath(pi.PropertyType, right);
                        return right == null ? null : left + "." + right;
                    }
                }
                return null;
            });

    /// <summary>
    /// Creates a ParallelOptions instance configured for parallel or sequential execution.
    /// </summary>
    /// <param name="canParallelize">
    /// If <c>true</c>, allows parallel execution; otherwise, forces sequential execution (MaxDegreeOfParallelism = 1).
    /// </param>
    /// <param name="degrees">
    /// Optional maximum degree of parallelism. Only used when <paramref name="canParallelize"/> is <c>true</c>.
    /// </param>
    /// <returns>A configured ParallelOptions instance.</returns>
    public static ParallelOptions CreateParallelOptions(bool canParallelize, int? degrees = null)
    {
        var po = new ParallelOptions();
        if (canParallelize)
        {
            if (degrees.HasValue)
            {
                po.MaxDegreeOfParallelism = degrees.Value;
            }
        }
        else
        {
            po.MaxDegreeOfParallelism = 1;
        }
        return po;
    }
}
