﻿using System.IO;
using System.Text;
using RevolutionaryStuff.Core.Streams;

namespace RevolutionaryStuff.Core;

public static class StreamHelpers
{
    public static async Task CopyFromAsync(this Stream st, string path)
    {
        Requires.ReadableStreamArg(st);
        Requires.FileExists(path);

        await using var src = File.OpenRead(path);
        await src.CopyToAsync(st);
    }

    public static void CopyFrom(this Stream st, string path)
    {
        Requires.ReadableStreamArg(st);
        Requires.FileExists(path);

        using var src = File.OpenRead(path);
        src.CopyTo(st);
    }

    public static void CopyTo(this Stream st, string path)
    {
        Requires.ReadableStreamArg(st);

        using var dst = File.Create(path);
        st.CopyTo(dst);
    }

    public static async Task CopyToAsync(this Stream st, string path)
    {
        Requires.ReadableStreamArg(st);

        await using var dst = File.Create(path);
        await st.CopyToAsync(dst);
    }

    public static async Task CopyToAsync(this Stream st, Stream dst, Action<int, long, long?> progress, int? bufferSize = null)
    {
        Requires.ReadableStreamArg(st);
        Requires.WriteableStreamArg(dst);

        var bufSize = bufferSize.GetValueOrDefault(1024 * 256);
        Requires.Positive(bufSize, nameof(bufferSize));

        var buf = new byte[bufSize];
        long tot = 0;
        long? len = 0;
        if (st.CanSeek)
        {
            len = st.Length - st.Position;
        }
        for (; ; )
        {
            var read = await st.ReadAsync(buf, 0, buf.Length);
            if (read <= 0) break;
            await dst.WriteAsync(buf, 0, read);
            tot += read;
            progress(read, tot, len);
        }
        progress(0, tot, len);
    }

    public static readonly UTF8Encoding UTF8EncodingWithoutPreamble = new(false);

    public static readonly UTF8Encoding UTF8EncodingWithPreamble = new(true);

    public static Stream CreateUtf8WithoutPreamble(string s)
        => Create(s, UTF8EncodingWithoutPreamble);

    public static Stream Create(string s, Encoding e = null)
    {
        var st = new MemoryStream();
        var sr = new StreamWriter(st, e ?? UTF8EncodingWithoutPreamble);
        sr.Write(s);
        sr.Flush();
        st.Position = 0;
        return st;
    }

    public static void Write(this Stream st, byte[] buffer)
    {
        if (buffer == null) return;
        st.Write(buffer, 0, buffer.Length);
    }

    public static async Task<string> ReadToEndAsync(this Stream st, Encoding enc = null)
    {
        using var sr = new StreamReader(new IndestructibleStream(st), enc ?? Encoding.UTF8);
        return await sr.ReadToEndAsync();
    }

    public static string ReadToEnd(this Stream st, Encoding enc = null)
    {
        using var sr = new StreamReader(new IndestructibleStream(st), enc ?? Encoding.UTF8);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// Seek via the Position member
    /// </summary>
    /// <remarks>
    /// Used for implemeters of a stream
    /// </remarks>
    /// <param name="st">The stream</param>
    /// <param name="offset">The offset</param>
    /// <param name="origin">The origin</param>
    /// <returns>The new position</returns>
    public static long SeekViaPos(this Stream st, long offset, SeekOrigin origin)
    {
        var p = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => st.Position + offset,
            SeekOrigin.End => st.Length + offset,
            _ => throw new UnexpectedSwitchValueException(origin),
        };
        st.Position = p;
        return p;
    }

    public static void ReadExactSize(this Stream st, byte[] buf, int offset = 0, int? size = null)
    {
        var remaining = size.GetValueOrDefault(buf.Length);
        while (remaining > 0)
        {
            var amtRead = st.Read(buf, offset, remaining);
            if (amtRead <= 0) break;
            remaining -= amtRead;
            offset += amtRead;
        }
        if (remaining > 0) throw new IndexOutOfRangeException($"{nameof(st)} was too small.  could not read remaining {remaining} bytes");
    }

    public static async Task<byte[]> ToBufferAsync(this Stream st)
    {
        Requires.ReadableStreamArg(st);

        if (st is not MemoryStream mst)
        {
            mst = new MemoryStream();
            await st.CopyToAsync(mst);
            mst.Position = 0;
        }
        return mst.ToArray();
    }

    public static void Write(this Stream st, string s, Encoding enc = null)
    {
        if (s == null) return;
        var buf = (enc ?? Encoding.UTF8).GetBytes(s);
        st.Write(buf, 0, buf.Length);
    }
}
