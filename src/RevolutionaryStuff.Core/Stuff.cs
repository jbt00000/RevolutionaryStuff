using Newtonsoft.Json;
using RevolutionaryStuff.Core.Caching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core
{
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

        public const int MAX_PATH = 255;

        public static readonly CultureInfo CultureUS = new CultureInfo("en-US");

        public static readonly string ApplicationName;

        public static readonly string ApplicationFamily;

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
                    Stuff.FileTryDelete(fn);
                }
            }
        }


        /// <summary>
        /// Constructs a name for a temporary file with the given extension
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <param name="tempPath">Temp path to use, null if you want System.GetTempPath</param>
        /// <returns>The name of the temp file</returns>
        public static string GetTempFileName(string extension, string tempPath = null, bool deleteAfterReservation = false)
        {
            Requires.FileExtension(extension, nameof(extension));
            string fn = $"{ApplicationInstanceId}.{Math.Abs(Environment.TickCount)}{extension}";
            if (string.IsNullOrEmpty(tempPath))
            {
                tempPath = Path.GetTempPath();
            }
            fn = Path.Combine(tempPath, fn);
            fn = FindOrigFileName(fn);
            if (deleteAfterReservation)
            {
                File.Delete(fn);
            }
            return fn;
        }

        private static readonly Regex NumberInParenExpr = new Regex(@" (\d*)$", RegexOptions.Compiled);

        /// <summary>
        /// Returns a filename similar to the given one if the given one already exists.
        /// The transformation may including adding (num) to the filename, or shortening it if the new path is too long.
        /// The extension and directoryname will remain unchanged
        /// </summary>
        /// <param name="path">The desired filename</param>
        /// <returns>A unique filename resembling the original filename</returns>
        public static string FindOrigFileName(string path)
        {
            string extension = null, name = null;
            int x = 1;
#if DEBUG
            if (RegexHelpers.Common.InvalidPathChars.IsMatch(path))
            {
                Noop();
            }
#endif
            path = RegexHelpers.Common.InvalidPathChars.Replace(path, " ");
            string dirName = Path.GetDirectoryName(path);            
            if (path.Length > MAX_PATH - 5)
            {
                string fn = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path);
                int delta = path.Length - (MAX_PATH - 7 - ext.Length);
                if (fn.Length > delta)
                {
                    fn = fn.Substring(0, fn.Length - delta);
                    fn = fn + "[+]" + ext;
                }
                path = Path.Combine(dirName, fn);
            }
            Directory.CreateDirectory(dirName);
            for (;;)
            {
                if (!File.Exists(path))
                {
                    try
                    {
                        using (var st = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            return path;
                        }
                    }
                    catch (IOException)
                    {
                    }
                }
                if (null == name)
                {
                    extension = Path.GetExtension(path);
                    name = Path.Combine(dirName, Path.GetFileNameWithoutExtension(path));
                    var m = NumberInParenExpr.Match(name);
                    if (m.Success)
                    {
                        int y = name.LastIndexOf(" (");
                        if (y >= 0)
                        {
                            name = name.Substring(0, x);
                            x = Convert.ToInt32(m.Groups[1].Value) + 1;
                        }
                    }
                }
                path = String.Format("{0} ({1}){2}", name, x++, extension);
            }
        }

        public static void FileTryDelete(params string[] fileNames)
            => FileTryDelete((IEnumerable<string>)fileNames);

        public static void FileTryDelete(IEnumerable<string> fileNames)
        {
            if (fileNames == null) return;
            foreach (var fn in fileNames)
            {
                if (string.IsNullOrEmpty(fn)) return;
                try
                {
                    File.Delete(fn);
                }
                catch (Exception) { }
            }
        }

        public static TResult ExecuteSynchronously<TResult>(this Task<TResult> task)
        {
            if (!task.IsCompleted)
            {
                try
                {
                    Task.WaitAll(task);
                }
                catch (AggregateException ae)
                {
                    throw ae.InnerException;
                }
            }
            return task.Result;
        }

        public static void ExecuteSynchronously(this Task task)
        {
            if (!task.IsCompleted)
            {
                try
                {
                    Task.WaitAll(task);
                }
                catch (AggregateException ae)
                {
                    throw ae.InnerException;
                }
            }
        }

        public static async Task TaskWhenAllForEach<TItem>(IEnumerable<TItem> items, Func<TItem, Task> taskCreator, int maxAtOnce = 2, [CallerMemberName] string caller = null)
        {
            var tasks = new List<Task>();
            long outstanding = 0;
            foreach (var item in items)
            {
                while (Interlocked.Read(ref outstanding) >= maxAtOnce)
                {
                    await Task.Delay(10);
                }
                Interlocked.Increment(ref outstanding);
                Debug.WriteLine($"{nameof(TaskWhenAllForEach)} from {caller}: Tot Started = {tasks.Count}; Outstanding = {outstanding}");
                var t = taskCreator(item);
                var tDone = t.ContinueWith(a => { Interlocked.Decrement(ref outstanding); return Task.CompletedTask; });
                tasks.Add(t);
            }
            while (Interlocked.Read(ref outstanding) > 0)
            {
                await Task.Delay(10);
            }
            Debug.WriteLine($"{nameof(TaskWhenAllForEach)} from {caller}: Tot Started = {tasks.Count}; Outstanding = {outstanding}");
            Debug.Assert(outstanding == 0);
            var exceptions = tasks.Where(z => z.Exception != null).Select(z => z.Exception).ToList();
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        public static void TaskWaitAllForEach<TSource>(IEnumerable<TSource> items, Func<TSource, Task> body, [CallerMemberName] string caller = null)
            => TaskWhenAllForEach(items, body, int.MaxValue, caller).ExecuteSynchronously();

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

        public static ParallelOptions CreateParallelOptions(bool canParallelize, int? degrees=null)
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
}
