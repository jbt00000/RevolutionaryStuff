﻿using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

/// <summary>
/// A Text Writer that sits on top of a pool of rotating files.
/// One specified the maximum number of files to keep around, and the max size of each individual file
/// </summary>
public class RotatingFileTextWriter : TextWriter
{
    private readonly Encoding Encoding_p;
    public readonly string FileNameFormat;
    public readonly int MaxFiles;
    public readonly int MaxLength;
    public bool AutoFlush;

    protected int CurrentFileNumber { get; private set; }
    protected long CurrentFileSize { get; private set; }

    protected readonly string FileNameSearch;
    protected readonly string FilePath;
    protected readonly Regex FileNumberExpr;
    protected StreamWriter Writer;

    #region Constructors

    public RotatingFileTextWriter(string fileNameFormat, int maxLength, int maxFiles, bool autoFlush = false, Encoding encoding = null)
    {
        AutoFlush = autoFlush;
        FileNameFormat = fileNameFormat;
        MaxFiles = maxFiles;
        MaxLength = maxLength;
        Encoding_p = encoding ?? Encoding.UTF8;

        FileNameFormat.Split(Path.DirectorySeparatorChar.ToString(), false, out FilePath, out FileNameSearch);
        var sExtractNum = Regex.Replace(FileNameSearch, "{.*}", "===;;;===");
        sExtractNum = Regex.Escape(sExtractNum);
        sExtractNum = Regex.Replace(sExtractNum, "===;;;===", "(.*)");
        FileNumberExpr = new Regex(sExtractNum, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        FileNameSearch = Regex.Replace(FileNameSearch, "{.*}", "*");
        var filenums = GetSortedFileNumbers();
        CurrentFileNumber = 0 == filenums.Length ? 0 : Math.Max(0, filenums[^1]);

        Clean();
    }

    protected override void Dispose(bool disposing)
    {
        Close();
        Clean();
        base.Dispose(disposing);
    }

    #endregion

    public override Encoding Encoding
    {
        [DebuggerStepThrough]
        get { return Encoding_p; }
    }

    public override void Write(char v)
    {
        PrepForWrite(1);
        Writer.Write(v);
    }

    public override void Write(string v)
    {
        v ??= "";
        PrepForWrite(v.Length);
        Writer.Write(v);
    }

    public override void WriteLine(string v)
    {
        v ??= "";
        PrepForWrite(v.Length);
        Writer.WriteLine(v);
    }

    protected void PrepForWrite(int size)
    {
        if (null == Writer || CurrentFileSize > MaxLength)
        {
            OpenWrite();
        }
        CurrentFileSize += size;
    }

    public override void Flush()
        => Writer?.Flush();

    public override void Close()
    {
        if (null != Writer)
        {
            Flush();
            Writer.Close();
            Writer = null;
        }
    }

    /// <summary>
    /// This event gets fired each time a new file is rotated
    /// </summary>
    public event EventHandler Rotation;

    protected void OpenWrite()
    {
        Close();

        for (; ; )
        {
            var fileName = CreateFileName();
            try
            {
                Stream st = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                Writer = new StreamWriter(st, Encoding) { AutoFlush = AutoFlush };
                CurrentFileSize = st.Length;
                Rotation?.Invoke(this, EventArgs.Empty);
                return;
            }
            catch (IOException)
            {
                ++CurrentFileNumber;
            }
        }
    }

    protected string CreateFileName()
    {
        var exists = false;
        string fileName;
        try
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
            var x = CurrentFileNumber;
            FileInfo f;
            for (; ; ++x)
            {
                fileName = string.Format(FileNameFormat, x);
                f = new FileInfo(fileName);
                if (File.Exists(fileName))
                {
                    if (f.Length < MaxLength)
                    {
                        exists = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            CurrentFileNumber = x;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
            fileName = null;
        }
        if (!exists)
        {
            Clean();
        }
        return fileName;
    }

    public string[] GetFileNames()
    {
        try
        {
            return Directory.GetFiles(FilePath, FileNameSearch);
        }
        catch (DirectoryNotFoundException)
        {
            return Empty.StringArray;
        }
    }

    protected int[] GetSortedFileNumbers()
    {
        var a = new List<int>();
        var files = GetFileNames();
        foreach (var filename in files)
        {
            try
            {
                var i = Convert.ToInt32(FileNumberExpr.GetGroupValue(filename));
                a.Add(i);
            }
            catch (Exception)
            {
            }
        }
        a.Sort();
        return a.ToArray();
    }

    protected void Clean()
    {
        var filenums = GetSortedFileNumbers();
        if (filenums.Length > MaxFiles)
        {
            for (var x = 0; x < filenums.Length - MaxFiles; ++x)
            {
                var Filename = string.Format(FileNameFormat, filenums[x]);
                try
                {
                    //Since this class is the base for RotatingLogTraceListener, we cannot call code that could potentially log, else we can get a stack overflow
                    File.Delete(Filename);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
