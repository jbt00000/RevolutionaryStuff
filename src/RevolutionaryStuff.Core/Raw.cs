using RevolutionaryStuff.Core.EncoderDecoders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RevolutionaryStuff.Core
{
    /// <summary>
    /// Routines that deal with either byte arrays, low level bit twiddling, and string conversion
    /// </summary>
    public static class Raw
    {
        public static TItem[] Xerox<TItem>(this TItem[] items) => (TItem[]) items.Clone();

        /// <summary>
        /// Create a new buffer that is a duplicate of the passed in buffer
        /// </summary>
        /// <param name="buf">The original buffer [opt]</param>
        /// <returns>A newly allocated buffer with a copy of the original data.  If original is null, return null</returns>
        public static byte[] Xerox(byte[] buf)
        {
            if (null == buf) return null;
            var b = new byte[buf.Length];
            buf.CopyTo(b, 0);
            return b;
        }

        /// <summary>
        /// Create a new buffer that is a duplicate of the subset of the passed in buffer we care about
        /// </summary>
        /// <param name="buf">The original buffer</param>
        /// <param name="offset">The offset into this buffer we want to copy from</param>
        /// <param name="size">The amount of data we want to copy</param>
        /// <returns>A newly allocated buffer with a copy of the original data we want to keep.  If original is null, return null</returns>
        public static byte[] Xerox(byte[] buf, int offset, int size)
        {
            if (null == buf) return null;
            if (size < 0 || offset < 0 || offset + size > buf.Length)
                throw new ArgumentException("the size/offset/length combo is bad");
            var b = new byte[size];
            Array.Copy(buf, offset, b, 0, size);
            return b;
        }

        public static string Buf2ASCIIString(byte[] buf)
        {
            return Buf2ASCIIString(buf, 0, buf.Length, false, true);
        }

        public static string Buf2ASCIIString(byte[] buf, bool Quoted)
        {
            return Buf2ASCIIString(buf, 0, buf.Length, false, Quoted);
        }

        /// <summary>
        /// Convert a buffer to an ASCII printable string for debugging
        /// Non-Printable characters are shown in their hexidecimal format
        /// </summary>
        /// <param name="buf">The buffer</param>
        /// <param name="offset">The offset into the buffer we start at</param>
        /// <param name="size">The amount of data we are printing</param>
        /// <param name="capitalizeHexidecimalCharacters">When true, capitalize the hex characters, else keep them lowercase</param>
        /// <param name="quoted">If quoted, enclose the string in "a'{0}'"</param>
        /// <returns>The buffer in debug printable format</returns>
        public static string Buf2ASCIIString(byte[] buf, long offset, long size, bool capitalizeHexidecimalCharacters,
                                             bool quoted)
        {
            var sb = new StringBuilder();
            if (quoted)
            {
                sb.Append("a'");
            }
            string format = capitalizeHexidecimalCharacters ? "{0:X}{1:X}" : "{0:x}{1:x}";
            if (null != buf)
            {
                long x;
                byte b;
                size += offset;
                for (x = offset; x < size; ++x)
                {
                    b = buf[x];
                    var ch = (char)b;
                    if (b >= 32 && b <= 'z')
                    {
                        sb.Append(" " + ch);
                    }
                    else
                    {
                        sb.AppendFormat(format, (b >> 4) & 0xf, b & 0xf);
                    }
                }
            }
            if (quoted)
            {
                sb.Append("'");
            }
            return sb.ToString();
        }

        [Obsolete("Use Base16.Encode instead", false)]
        public static string Buf2HexString(byte[] buf)
        {
            return Buf2HexString(buf, false, true);
        }

        public static string Buf2HexString(byte[] buf, bool fCaps, bool fQuoted)
        {
            return Buf2HexString(buf, 0, buf.Length, false, fQuoted);
        }

        public static string Buf2HexString(byte[] buf, int offset, int size, bool fCaps, bool fQuoted)
        {
            string s = Base16.Encode(buf, offset, size, fCaps);
            return fQuoted ? String.Format("x'{0}'", s) : s;
        }

        /// <summary>
        /// Converts a string to a buffer using UTF-8 encoding
        /// </summary>
        /// <param name="s">The string</param>
        /// <returns>A buffer with the string</returns>
        public static byte[] String2Buf(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        public static byte[] ToUTF8(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        public static string Buf2String(byte[] buf)
        {
            return Buf2String(buf, 0, buf.Length, (Decoder)null);
        }

        public static string Buf2String(byte[] buf, Decoder d)
        {
            return Buf2String(buf, 0, buf.Length, d);
        }

        public static string Buf2String(byte[] buf, int offset, int length)
        {
            return Buf2String(buf, offset, length, (Decoder)null);
        }

        public static string Buf2String(byte[] buf, int offset, int length, Decoder d)
        {
            if (length == 0) return "";
            if (null == d)
            {
                d = Encoding.UTF8.GetDecoder();
            }
            var cbuf = new char[d.GetCharCount(buf, offset, length)];
#if DEBUG
            if (cbuf.Length != length)
            {
                Stuff.Noop();
            }
#endif
            d.GetChars(buf, offset, length, cbuf, 0);
            return new String(cbuf, 0, cbuf.Length);
        }

        public static string Buf2String(byte[] buf, int offset, int length, Encoding d)
        {
            if (null == d)
            {
                d = Encoding.UTF8;
            }
            var cbuf = new char[d.GetCharCount(buf, offset, length)];
#if DEBUG
            if (cbuf.Length != length)
            {
                Stuff.Noop();
            }
#endif
            d.GetChars(buf, offset, length, cbuf, 0);
            return new String(cbuf, 0, cbuf.Length);
        }

        /// <summary>
        /// Returns an ascii string from a null terminated buffer
        /// </summary>
        /// <param name="buf">The buffer where the string is stored</param>
        /// <param name="offset">The offset from the buffer where the read begins</param>
        /// <returns>The string or null if there was no null termination</returns>
        public static string Buf2AsciiSZ(byte[] buf, int offset = 0)
        {
            return Buf2AsciiSZ(buf, offset, buf.Length - offset);
        }

        /// <summary>
        /// Returns an ascii string from a null terminated buffer
        /// </summary>
        /// <param name="buf">The buffer where the string is stored</param>
        /// <param name="offset">The offset from the buffer where the read begins</param>
        /// <param name="max">The maximum number of characters this string can contain</param>
        /// <returns>The string or null if there was no null termination</returns>
        public static string Buf2AsciiSZ(byte[] buf, int offset, int max)
        {
            max = Math.Min(max, buf.Length - offset);
            int x = Array.IndexOf(buf, (byte)0, offset, max);
            if (x < 0) return null;
            if (0 == x) return "";
            return Buf2String(buf, offset, x - offset);
        }

        /// <summary>
        /// Joins a list of buffers into a single one
        /// </summary>
        /// <param name="buffers">The list of buffers</param>
        /// <returns>A single buffer that is the concatenation of each individual buffer</returns>
        public static byte[] BufListJoin(IList buffers)
        {
            int x, totlen = 0, len = buffers.Count;
            byte[] buf;
            for (x = 0; x < len; ++x)
            {
                buf = (byte[])buffers[x];
                totlen += buf.Length;
            }
            var ret = new byte[totlen];
            totlen = 0;
            for (x = 0; x < len; ++x)
            {
                buf = (byte[])buffers[x];
                buf.CopyTo(ret, totlen);
                totlen += buf.Length;
            }
            return ret;
        }

        public static byte[] BufReplaceSection(byte[] buf, int offset, int length, byte[] bufReplace)
        {
            var bout = new byte[bufReplace.Length - length + buf.Length];
            Array.Copy(buf, 0, bout, 0, offset);
            bufReplace.CopyTo(bout, offset);
            Array.Copy(buf, offset + length, bout, offset + bufReplace.Length, buf.Length - (offset + length));
            return bout;
        }

        public static int ReadInt32FromBuf(byte[] buf, int offset)
        {
            return
                (buf[offset + 3] << 24) |
                (buf[offset + 2] << 16) |
                (buf[offset + 1] << 8) |
                (buf[offset + 0]);
        }

        public static int ReadInt32BeFromBuf(byte[] buf, int offset)
        {
            return
                (buf[offset + 0] << 24) |
                (buf[offset + 1] << 16) |
                (buf[offset + 2] << 8) |
                (buf[offset + 3]);
        }

        public static long ReadInt64FromBuf(byte[] buf, int offset)
        {
            ulong ul =
                (((ulong)buf[offset + 7]) << (8 * 7)) |
                (((ulong)buf[offset + 6]) << (8 * 6)) |
                (((ulong)buf[offset + 5]) << (8 * 5)) |
                (((ulong)buf[offset + 4]) << (8 * 4)) |
                (((ulong)buf[offset + 3]) << (8 * 3)) |
                (((ulong)buf[offset + 2]) << (8 * 2)) |
                (((ulong)buf[offset + 1]) << (8 * 1)) |
                (((ulong)buf[offset + 0]) << (8 * 0));
            return (long)ul;
        }

        public static void WriteInt32ToBuf(byte[] buf, int offset, int val)
        {
            buf[offset + 0] = (byte)((val >> 0) & 0xff);
            buf[offset + 1] = (byte)((val >> 8) & 0xff);
            buf[offset + 2] = (byte)((val >> 16) & 0xff);
            buf[offset + 3] = (byte)((val >> 24) & 0xff);
            Debug.Assert(val == ReadInt32FromBuf(buf, offset));
        }

        public static void WriteInt32BeToBuf(byte[] buf, int offset, int val)
        {
            buf[offset + 3] = (byte)((val >> 0) & 0xff);
            buf[offset + 2] = (byte)((val >> 8) & 0xff);
            buf[offset + 1] = (byte)((val >> 16) & 0xff);
            buf[offset + 0] = (byte)((val >> 24) & 0xff);
            Debug.Assert(val == ReadInt32BeFromBuf(buf, offset));
        }

        public static void WriteUInt32ToBuf(byte[] buf, int offset, uint val)
        {
            buf[offset + 0] = (byte)((val >> 0) & 0xff);
            buf[offset + 1] = (byte)((val >> 8) & 0xff);
            buf[offset + 2] = (byte)((val >> 16) & 0xff);
            buf[offset + 3] = (byte)((val >> 24) & 0xff);
            Debug.Assert(val == ReadUInt32FromBuf(buf, offset));
        }

        public static void WriteInt64ToBuf(byte[] buf, int offset, long val)
        {
            buf[offset + 0] = (byte)((val >> (8 * 0)) & 0xff);
            buf[offset + 1] = (byte)((val >> (8 * 1)) & 0xff);
            buf[offset + 2] = (byte)((val >> (8 * 2)) & 0xff);
            buf[offset + 3] = (byte)((val >> (8 * 3)) & 0xff);
            buf[offset + 4] = (byte)((val >> (8 * 4)) & 0xff);
            buf[offset + 5] = (byte)((val >> (8 * 5)) & 0xff);
            buf[offset + 6] = (byte)((val >> (8 * 6)) & 0xff);
            buf[offset + 7] = (byte)((val >> (8 * 7)) & 0xff);
            Debug.Assert(val == ReadInt64FromBuf(buf, offset));
        }

        public static uint ReadUInt32FromBuf(byte[] buf, int offset)
        {
            return (uint)ReadInt32FromBuf(buf, offset);
        }

        public static UInt16 ReadUInt16FromBuf(byte[] buf, int offset)
        {
            return (ushort)((buf[offset + 1] << 8) | buf[offset]);
        }

        public static Int16 ReadInt16BeFromBuf(byte[] buf, int offset)
        {
            return (short)((buf[offset] << 8) | buf[offset + 1]);
        }

        public static Int16 ReadInt16FromBuf(byte[] buf, int offset)
        {
            return (short)((buf[offset + 1] << 8) | buf[offset]);
        }

        public static void WriteUInt16ToBuf(byte[] buf, int offset, UInt16 val)
        {
            buf[offset + 0] = (byte)((val >> 0) & 0xff);
            buf[offset + 1] = (byte)((val >> 8) & 0xff);
            Debug.Assert(val == ReadUInt16FromBuf(buf, offset));
        }

        public static void WriteInt16ToBuf(byte[] buf, int offset, Int16 val)
        {
            buf[offset + 0] = (byte)((val >> 0) & 0xff);
            buf[offset + 1] = (byte)((val >> 8) & 0xff);
            Debug.Assert(val == ReadInt16FromBuf(buf, offset));
        }

        public static byte[] Num2Buf(int num)
        {
            var buf = new byte[4];
            WriteInt32ToBuf(buf, 0, num);
            return buf;
        }

        public static byte[] Num2Buf(long num)
        {
            var buf = new byte[8];
            WriteInt64ToBuf(buf, 0, num);
            return buf;
        }

        public static uint XORBytes(byte[] buf)
        {
            return XORBytes(buf, 0, buf.Length);
        }

        public static uint XORBytes(byte[] buf, int offset, int size)
        {
            Debug.Assert((size & 0x3) == 0);
            uint u = 0;
            while (size > 0)
            {
                size -= 4;
                u ^= (uint)ReadInt32FromBuf(buf, offset + size);
            }
            return u;
        }

        public static int TruncateToInt(long val)
        {
            return val > int.MaxValue ? int.MaxValue : (int)val;
        }

        public static long MakeLong(int hi, int lo)
        {
            return (long)MakeULong((uint)hi, (uint)lo);
        }

        public static ulong MakeULong(uint hi, uint lo)
        {
            return (((ulong)hi) << 32) | lo;
        }

        public static uint MakeUInt(ushort hi, ushort lo)
        {
            return (uint)((hi << 16) | lo);
        }

        public static UInt16 GetHiWord(uint val)
        {
            return (UInt16)(val >> 16);
        }

        public static UInt16 GetLoWord(uint val)
        {
            return (UInt16)(val & 0xffff);
        }

        public static UInt32 GetHiInt(ulong val)
        {
            return (UInt32)(val >> 32);
        }

        public static UInt32 GetLoInt(ulong val)
        {
            return (UInt32)(val & 0xffffffff);
        }

        public static byte MakeByte(byte hi, byte lo)
        {
            Debug.Assert(0 == (hi & 0xff00));
            Debug.Assert(0 == (lo & 0xff00));
            return (byte)((hi << 4) | lo);
        }

        public static SByte GetHiSNibble(byte b)
        {
            return GetLoSNibble((byte)(b >> 4));
        }

        public static SByte GetLoSNibble(byte b)
        {
            if (0 == (0x8 & b))
            {
                return (SByte)(b & 0x7);
            }
            else
            {
                return (SByte)(0xF0 | b);
            }
        }

        public static int SwapEndianInt32(int i)
        {
            int a, b, c, d;
            a = i & 0xff;
            b = (i >> 8) & 0xff;
            c = (i >> 16) & 0xff;
            d = (i >> 24) & 0xff;
            return (a << 24) | (b << 16) | (c << 8) | (d);
        }

        public static Int16 SwapEndianInt16(Int16 i)
        {
            int a, b;
            a = i & 0xff;
            b = (i >> 8) & 0xff;
            return (Int16)((a << 8) | (b));
        }

        public static byte[] NetmonText2Buf(string sText)
        {
            string[] sLines = RegexHelpers.Common.N.Split(sText.ToLower());
            string sLine;
            int x, y;
            byte ba, bb;
            byte[] bLine;
            var a = new List<byte>(sLines.Length*16);
            for (y = 0; y < sLines.Length; ++y)
            {
                try
                {
                    sLine = sLines[y].Substring(10, 16*3 - 1);
                    bLine = Encoding.ASCII.GetBytes(sLine);

                    for (x = 0; x < 16; ++x)
                    {
                        try
                        {
                            ba = ASCII.HexCharAsByte2Val(bLine[3*x + 0]);
                            bb = ASCII.HexCharAsByte2Val(bLine[3*x + 1]);
                            if (ba != byte.MaxValue && bb != byte.MaxValue)
                            {
                                a.Add((byte) (ba*16 + bb));
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                catch (Exception)
                {
                    y = sLines.Length;
                }
            }
            return a.ToArray();
        }

        #region Nested type: ASCII

        internal class ASCII
        {
            public static readonly byte byte0 = 48;
            public static readonly byte byte9 = 57;
            public static readonly byte bytea = 97;
            public static readonly byte byteA = 65;
            public static readonly byte bytef = 102;
            public static readonly byte byteF = 70;

            public static byte HexCharAsByte2Val(byte b)
            {
                if (b >= byte0 && b <= byte9)
                {
                    return (byte)(b - byte0);
                }
                else if (b >= bytea && b <= bytef)
                {
                    return (byte)((b - bytea) + 10);
                }
                else if (b >= byteA && b <= byteF)
                {
                    return (byte)((b - byteA) + 10);
                }
                return byte.MaxValue;
            }
        }

        #endregion
    }
}