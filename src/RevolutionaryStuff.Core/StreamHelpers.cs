using System.IO;
using System.Text;
using RevolutionaryStuff.Core.Streams;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides utility methods for working with streams, including copying, reading, writing, and conversion operations.
/// </summary>
public static class StreamHelpers
{
    /// <summary>
    /// Asynchronously copies data from a file into the specified stream.
    /// </summary>
    /// <param name="st">The destination stream to copy data into.</param>
    /// <param name="path">The path to the source file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is not readable or the file does not exist.</exception>
    public static async Task CopyFromAsync(this Stream st, string path)
    {
        Requires.ReadableStreamArg(st);
        Requires.FileExists(path);

        await using var src = File.OpenRead(path);
        await src.CopyToAsync(st);
    }

    /// <summary>
    /// Synchronously copies data from a file into the specified stream.
    /// </summary>
    /// <param name="st">The destination stream to copy data into.</param>
    /// <param name="path">The path to the source file.</param>
    /// <exception cref="ArgumentException">Thrown when the stream is not readable or the file does not exist.</exception>
    public static void CopyFrom(this Stream st, string path)
    {
        Requires.ReadableStreamArg(st);
        Requires.FileExists(path);

        using var src = File.OpenRead(path);
        src.CopyTo(st);
    }

    /// <summary>
    /// Synchronously copies data from the stream to a file.
    /// </summary>
    /// <param name="st">The source stream to copy data from.</param>
    /// <param name="path">The path to the destination file.</param>
    /// <exception cref="ArgumentException">Thrown when the stream is not readable.</exception>
    public static void CopyTo(this Stream st, string path)
    {
        Requires.ReadableStreamArg(st);

        using var dst = File.Create(path);
        st.CopyTo(dst);
    }

    /// <summary>
    /// Asynchronously copies data from the stream to a file.
    /// </summary>
    /// <param name="st">The source stream to copy data from.</param>
    /// <param name="path">The path to the destination file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is not readable.</exception>
    public static async Task CopyToAsync(this Stream st, string path)
    {
        Requires.ReadableStreamArg(st);

        await using var dst = File.Create(path);
        await st.CopyToAsync(dst);
    }

    /// <summary>
    /// Asynchronously copies data from one stream to another with progress reporting.
    /// </summary>
    /// <param name="st">The source stream to copy data from.</param>
    /// <param name="dst">The destination stream to copy data to.</param>
    /// <param name="progress">
    /// A callback invoked after each read operation, receiving:
    /// (bytesRead, totalBytesRead, totalLength).
    /// The final call will have bytesRead = 0 to signal completion.
    /// </param>
    /// <param name="bufferSize">The size of the buffer to use for copying. Defaults to 256KB.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when streams are not readable/writable or buffer size is invalid.</exception>
    public static async Task CopyToAsync(this Stream st, Stream dst, Action<int, long, long?> progress, int? bufferSize = null)
    {
        Requires.ReadableStreamArg(st);
        Requires.WriteableStreamArg(dst);

        var bufSize = bufferSize.GetValueOrDefault(1024 * 256);
        Requires.Positive(bufSize, nameof(bufferSize));

        var buf = new byte[bufSize];
        long tot = 0;
        long? len = null;
        if (st.CanSeek)
        {
            len = st.Length - st.Position;
        }
        for (; ; )
        {
            var read = await st.ReadAsync(buf.AsMemory(0, buf.Length));
            if (read <= 0) break;
            await dst.WriteAsync(buf.AsMemory(0, read));
            tot += read;
            progress(read, tot, len);
        }
        progress(0, tot, len);
    }

    /// <summary>
    /// UTF-8 encoding without byte order mark (BOM/preamble).
    /// </summary>
    public static readonly UTF8Encoding UTF8EncodingWithoutPreamble = new(false);

    /// <summary>
    /// UTF-8 encoding with byte order mark (BOM/preamble).
    /// </summary>
    public static readonly UTF8Encoding UTF8EncodingWithPreamble = new(true);

    /// <summary>
    /// Creates a stream from a string using UTF-8 encoding without BOM.
    /// </summary>
    /// <param name="s">The string to convert to a stream.</param>
    /// <returns>A readable <see cref="MemoryStream"/> containing the encoded string.</returns>
    public static Stream CreateUtf8WithoutPreamble(string s)
        => Create(s, UTF8EncodingWithoutPreamble);

    /// <summary>
    /// Creates a stream from a string using the specified encoding.
    /// </summary>
    /// <param name="s">The string to convert to a stream.</param>
    /// <param name="e">The encoding to use. Defaults to UTF-8 without BOM.</param>
    /// <returns>A readable <see cref="MemoryStream"/> containing the encoded string.</returns>
    public static Stream Create(string s, Encoding e = null)
    {
        var st = new MemoryStream();
        var sr = new StreamWriter(st, e ?? UTF8EncodingWithoutPreamble);
        sr.Write(s);
        sr.Flush();
        st.Position = 0;
        return st;
    }

    /// <summary>
    /// Writes the entire byte array to the stream.
    /// </summary>
    /// <param name="st">The stream to write to.</param>
    /// <param name="buffer">The byte array to write. If null, no operation is performed.</param>
    public static void Write(this Stream st, byte[] buffer)
    {
        if (buffer == null) return;
        st.Write(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Asynchronously reads all text from the stream using the specified encoding.
    /// The stream is not disposed after reading.
    /// </summary>
    /// <param name="st">The stream to read from.</param>
    /// <param name="enc">The encoding to use for reading. Defaults to UTF-8.</param>
    /// <returns>A task containing the complete text content of the stream.</returns>
    public static async Task<string> ReadToEndAsync(this Stream st, Encoding enc = null)
    {
        using var sr = new StreamReader(new IndestructibleStream(st), enc ?? Encoding.UTF8);
        return await sr.ReadToEndAsync();
    }

    /// <summary>
    /// Synchronously reads all text from the stream using the specified encoding.
    /// The stream is not disposed after reading.
    /// </summary>
    /// <param name="st">The stream to read from.</param>
    /// <param name="enc">The encoding to use for reading. Defaults to UTF-8.</param>
    /// <returns>The complete text content of the stream.</returns>
    public static string ReadToEnd(this Stream st, Encoding enc = null)
    {
        using var sr = new StreamReader(new IndestructibleStream(st), enc ?? Encoding.UTF8);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// Seeks to a position in the stream using the Position property.
    /// Intended for use by stream implementers.
    /// </summary>
    /// <param name="st">The stream to seek within.</param>
    /// <param name="offset">The offset relative to the origin.</param>
    /// <param name="origin">The reference point for seeking.</param>
    /// <returns>The new position in the stream.</returns>
    /// <exception cref="UnexpectedSwitchValueException">Thrown when an invalid SeekOrigin is provided.</exception>
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

    /// <summary>
    /// Reads exactly the specified number of bytes from the stream.
    /// </summary>
    /// <param name="st">The stream to read from.</param>
    /// <param name="buf">The buffer to read data into.</param>
    /// <param name="offset">The offset in the buffer to start writing data. Defaults to 0.</param>
    /// <param name="size">The number of bytes to read. Defaults to the buffer length.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the stream ends before the requested number of bytes can be read.
    /// </exception>
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

    /// <summary>
    /// Asynchronously reads the entire stream into a byte array.
    /// If the stream is already a <see cref="MemoryStream"/>, its buffer is returned directly.
    /// </summary>
    /// <param name="st">The stream to read from.</param>
    /// <returns>A task containing a byte array with the complete stream contents.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is not readable.</exception>
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

    /// <summary>
    /// Writes a string to the stream using the specified encoding.
    /// </summary>
    /// <param name="st">The stream to write to.</param>
    /// <param name="s">The string to write. If null, no operation is performed.</param>
    /// <param name="enc">The encoding to use. Defaults to UTF-8.</param>
    public static void Write(this Stream st, string s, Encoding enc = null)
    {
        if (s == null) return;
        var buf = (enc ?? Encoding.UTF8).GetBytes(s);
        st.Write(buf, 0, buf.Length);
    }
}
