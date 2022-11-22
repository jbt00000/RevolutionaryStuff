namespace RevolutionaryStuff.Core.EncoderDecoders;

/// <summary>
/// Routines to encode/decode buffers using Base64 encoding
/// </summary>
public static class Base64
{
    /// <summary>
    /// Encode a buffer using the default options
    /// </summary>
    /// <param name="buf">The buffer</param>
    /// <returns>An encoded string that holds the contents of the inputs</returns>
    public static string Encode(byte[] buf)
        => Encode(buf, 0, buf == null ? 0 : buf.Length);

    /// <summary>
    /// Encode part of a buffer using the default options
    /// </summary>
    /// <param name="buf">The buffer</param>
    /// <param name="offset">The offset into the buffer</param>
    /// <param name="length">The amount of data to encode</param>
    /// <returns>An encoded string that holds the contents of the inputs</returns>
    public static string Encode(byte[] buf, int offset, int length)
        => Convert.ToBase64String(buf, offset, length);

    /// <summary>
    /// Decode a Base64 string into a byte array
    /// </summary>
    /// <param name="s">A Base64 string</param>
    /// <returns>The decoded byte array</returns>
    public static byte[] Decode(string s)
    {
        s = s.TrimOrNull();
        if (s == null) return Empty.ByteArray;
        return Convert.FromBase64String(s);
    }

    public static string ToBase64String(this byte[] buf)
        => Encode(buf);
}
