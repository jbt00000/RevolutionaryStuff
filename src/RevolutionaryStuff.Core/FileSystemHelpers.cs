﻿using System.IO;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

public static partial class FileSystemHelpers
{
    public const int MAX_PATH = 255;

    public static string GetParentDirectory(string dir)
    {
        dir = dir.TrimOrNull();
        var i = dir.LastIndexOf(Path.DirectorySeparatorChar);
        if (i == dir.Length - 1)
        {
            dir = dir[..^1];
            i = dir.LastIndexOf(Path.DirectorySeparatorChar);
        }
        var ch = dir[^1];
        if (ch != ':' && ch != Path.DirectorySeparatorChar)
        {
            dir = dir[..i];
            if (dir[^1] == ':')
            {
                dir += Path.DirectorySeparatorChar;
            }
            return dir;
        }
        return null;
    }

    /// <summary>
    /// Constructs a name for a temporary file with the given extension
    /// </summary>
    /// <param name="extension">The file extension</param>
    /// <param name="tempPath">Temp path to use, null if you want System.GetTempPath</param>
    /// <returns>The name of the temp file</returns>
    public static string GetTempFileName(string extension, string tempPath = null, bool deleteAfterReservation = false)
    {
        Requires.FileExtension(extension);
        var fn = $"{Stuff.ApplicationInstanceId}.{Math.Abs(Environment.TickCount)}{extension}";
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

    [GeneratedRegex(@"(\d*)$")]
    private static partial Regex NumberInParenExpr { get; }

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
        var x = 1;
#if DEBUG
        if (RegexHelpers.Common.InvalidPathChars.IsMatch(path))
        {
            Stuff.NoOp();
        }
#endif
        path = RegexHelpers.Common.InvalidPathChars.Replace(path, " ");
        var dirName = Path.GetDirectoryName(path);
        if (path.Length > MAX_PATH - 5)
        {
            var fn = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);
            var delta = path.Length - (MAX_PATH - 7 - ext.Length);
            if (fn.Length > delta)
            {
                fn = fn[..^delta];
                fn = fn + "[+]" + ext;
            }
            path = Path.Combine(dirName, fn);
        }
        Directory.CreateDirectory(dirName);
        for (; ; )
        {
            if (!File.Exists(path))
            {
                try
                {
                    using var st = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    return path;
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
                    var y = name.LastIndexOf(" (");
                    if (y >= 0)
                    {
                        name = name[..x];
                        x = Convert.ToInt32(m.Groups[1].Value) + 1;
                    }
                }
            }
            path = string.Format("{0} ({1}){2}", name, x++, extension);
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

}
