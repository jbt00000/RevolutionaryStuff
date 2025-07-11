﻿using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core;

public static class Stuff
{
    public static readonly Assembly ThisAssembly;

    /// <summary>
    /// A default logger you can set that will be used primary by static functions.  
    /// Setting this to null will instead set to the NullLogger.Instance
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
    /// In case we need to cuss someone out in a polite fashion!
    /// </summary>
    public const string Qbert = "@!#?@!";

    public const string BaseRsllcUrn = "urn:www.revolutionarystuff.com";

    internal const string ConfigSectionNamePrefix = "Rsllc";

    public static readonly string ApplicationName;

    public static readonly string ApplicationFamily;

    public static readonly DateTimeOffset ApplicationStartedAt = DateTimeOffset.UtcNow;

    public static readonly Guid ApplicationInstanceId = Guid.NewGuid();

    /// <summary>
    /// Random number generator with a fixed seed.  Useful for testing.
    /// </summary>
    public static readonly Random RandomWithFixedSeed = new(19740409);

    /// <summary>
    /// Random number generator with a random seed value.
    /// </summary>
    public static readonly Random RandomWithRandomSeed = new(Crypto.Salt.RandomInteger);

    /// <summary>
    /// Instance of a random number generator
    /// </summary>
    public static readonly Random Random = RandomWithRandomSeed;

    /// <summary>
    /// Does nothing.  It is simply used as a line where one can set breakpoints
    /// </summary>
    /// <param name="args">Pass in parameters if you don't want them compiled out</param>
    [Conditional("DEBUG")]
    public static void NoOp(params object[] args)
    { }

    public static string ToString(object o)
        => o?.ToString();

    public static void Swap<T>(ref T a, ref T b)
        => (b, a) = (a, b);

    public static T Min<T>(T a, T b) where T : IComparable<T>
        => a.CompareTo(b) < 0 ? a : b;

    public static T Max<T>(T a, T b) where T : IComparable<T>
        => a.CompareTo(b) < 0 ? b : a;

    /// <summary>
    /// Convert a tickcount that was created in this windows session to a date time
    /// </summary>
    /// <param name="TickCount">The tickcount</param>
    /// <returns>The datetime when this happened</returns>
    public static DateTime TickCount2DateTime(int tickCount)
    {
        var n = DateTime.Now;
        var tc = Environment.TickCount;
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

    internal static string ObjectToString(object o, string fallback = null)
        => o?.ToString() ?? fallback;

    private static readonly IList<string> FilesToDeleteOnExit = [];

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

    public static string CreateRandomCode(int numChars = 6)
        => Services.CodeStringGenerator.DefaultCodeStringGenerator.Instance.CreateCaptchaCharactersCode(numChars);

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
