using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core;

public static class Stuff
{
    public static readonly Assembly ThisAssembly;

    static Stuff()
    {
        ThisAssembly = typeof(Stuff).GetTypeInfo().Assembly;
        var a = Assembly.GetEntryAssembly();
        var info = a?.GetInfo();
        ApplicationName = StringHelpers.Coalesce(info?.Title, a?.GetName().Name, "Unnamed");
        ApplicationFamily = StringHelpers.Coalesce(info?.Product, info?.Company, ApplicationName);
    }

    public const string Qbert = "@!#?@!";

    public const string BaseRsllcUrn = "urn:www.revolutionarystuff.com";

    public static readonly CultureInfo CultureUS = new CultureInfo("en-US");

    public static readonly string ApplicationName;

    public static readonly string ApplicationFamily;

    public static readonly DateTimeOffset ApplicationStartedAt = DateTimeOffset.UtcNow;

    public static readonly Guid ApplicationInstanceId = Guid.NewGuid();

    /// <summary>
    /// Random number generator with a fixed seed.  Useful for testing.
    /// </summary>
    public static readonly Random RandomWithFixedSeed = new Random(19740409);

    /// <summary>
    /// Random number generator with a random seed value.
    /// </summary>
    public static readonly Random RandomWithRandomSeed = new Random(Crypto.Salt.RandomInteger);

    /// <summary>
    /// Instance of a random number generator
    /// </summary>
    public static readonly Random Random = RandomWithRandomSeed;

    /// <summary>
    /// Does nothing.  It is simply used as a line where one can set breakpoints
    /// </summary>
    /// <param name="args">Pass in parameters if you don't want them compiled out</param>
    [Conditional("DEBUG")]
    public static void Noop(params object[] args)
    {
    }

    public static string ToString(object o)
        => o == null ? null : o.ToString();

    private static readonly JsonSerializerSettings ToJsonJsonSerializerSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    public static string ToJson(object o)
        => JsonConvert.SerializeObject(o, Formatting.Indented, ToJsonJsonSerializerSettings);

    [Obsolete("Use StringHelpers.Coalesce", false)]
    public static string CoalesceStrings(params string[] vals)
        => StringHelpers.Coalesce(vals);

    public static void Swap<T>(ref T a, ref T b)
    {
        T t = a;
        a = b;
        b = t;
    }

    public static T Min<T>(T a, T b) where T : IComparable<T>
    {
        return a.CompareTo(b) < 0 ? a : b;
    }

    public static T Max<T>(T a, T b) where T : IComparable<T>
    {
        return a.CompareTo(b) < 0 ? b : a;
    }

    /// <summary>
    /// Convert a tickcount that was created in this windows session to a date time
    /// </summary>
    /// <param name="TickCount">The tickcount</param>
    /// <returns>The datetime when this happened</returns>
    public static DateTime TickCount2DateTime(int tickCount)
    {
        DateTime n = DateTime.Now;
        int tc = Environment.TickCount;
        return n.AddMilliseconds(tickCount - tc);
    }

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
    /// Is the enum that is marked with the FlagsAttribute equal to the passed in value
    /// </summary>
    /// <param name="Flags">The flags enum</param>
    /// <param name="Val">The value we are testing against</param>
    /// <returns>True if the test and the flag are the same, else false</returns>
    public static bool FlagEq(Enum Flags, Enum Val)
    {
        var flags = (long)Convert.ChangeType(Flags, typeof(long), null);
        var val = (long)Convert.ChangeType(Val, typeof(long), null);
        return val == (flags & val);
    }

    /// <summary>
    /// Dispose an object if it has an IDisposable interface
    /// </summary>
    /// <param name="o">The object</param>
    public static void Dispose(params object[] os)
    {
        if (os == null) return;
        foreach (object o in os)
        {
            var d = o as IDisposable;
            if (d == null) return;
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

    public static string ObjectToString(object o, string fallback = null)
        => o?.ToString() ?? fallback;

    private static readonly IList<string> FilesToDeleteOnExit = new List<string>();

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

    public static string GetPathFromSerializedPath(Type t, string serializedPath)
        => Cache.DataCacher.FindOrCreateValue(
            Cache.CreateKey(typeof(Stuff), nameof(GetPathFromSerializedPath), t, serializedPath),
            () =>
            {
                if (serializedPath == null) return null;
                string left = serializedPath.LeftOf(".");
                string right = StringHelpers.TrimOrNull(serializedPath.RightOf("."));

                foreach (var pi in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (pi.GetCustomAttribute<Newtonsoft.Json.JsonIgnoreAttribute>() != null) continue;
                    var jpn = pi.GetCustomAttribute<Newtonsoft.Json.JsonPropertyAttribute>();
                    if ((jpn == null && pi.Name == left) || (jpn != null && jpn.PropertyName == left))
                    {
                        left = pi.Name;
                        if (right == null) return left;
                        right = GetPathFromSerializedPath(pi.PropertyType, right);
                        if (right == null) return null;
                        return left + "." + right;
                    }
                }
                return null;
            });

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
