using System.Diagnostics;
using System.Text;

namespace RevolutionaryStuff.Core.EncoderDecoders;

/// <summary>
/// Routines to encode/decode buffers using Base16 encoding
/// </summary>
public static class Base16
{
    /// <summary>
    /// If by default, the hex characters should be capitalized
    /// </summary>
    public static bool Caps = true;

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
        => Encode(buf, offset, length, Caps);

    /// <summary>
    /// Encode part of a buffer
    /// </summary>
    /// <param name="buf">The buffer</param>
    /// <param name="offset">The offset into the buffer</param>
    /// <param name="length">The amount of data to encode</param>
    /// <param name="caps">Whether or not the hex characters should be capitalized</param>
    /// <returns>An encoded string that holds the contents of the inputs</returns>
    public static string Encode(byte[] buf, int offset, int length, bool caps)
    {
        Debug.Assert(offset + length <= buf.Length);
        var sb = new StringBuilder();
        var format = caps ? "{0:X}{1:X}" : "{0:x}{1:x}";
        if (null != buf)
        {
            int x;
            byte b;
            for (x = 0; x < length; ++x)
            {
                b = buf[x + offset];
                sb.AppendFormat(format, (b >> 4) & 0xf, b & 0xf);
            }
        }
        return sb.ToString();
    }

    public static string ToBase16String(this byte[] buf)
        => Encode(buf);

    /// <summary>
    /// Decode a hex string into a byte array
    /// </summary>
    /// <param name="s">A hex string</param>
    /// <returns>The decoded byte array</returns>
    public static byte[] Decode(string s)
    {
        s = s.TrimOrNull();
        if (s == null) return Empty.ByteArray;

        var bLine = Encoding.ASCII.GetBytes(s);
        var buf = new byte[bLine.Length / 2];
        byte ba, bb;
        int x;
        for (x = 0; x < buf.Length; ++x)
        {
            ba = Raw.ASCII.HexCharAsByte2Val(bLine[2 * x + 0]);
            bb = Raw.ASCII.HexCharAsByte2Val(bLine[2 * x + 1]);
            if (ba != byte.MaxValue && bb != byte.MaxValue)
            {
                buf[x] = (byte)(ba * 16 + bb);
            }
        }
        return buf;
    }
}
