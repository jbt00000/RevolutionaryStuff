﻿using RevolutionaryStuff.Core.Crypto;
using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

namespace RevolutionaryStuff.Core.Caching
{
    public static class Cache
    {
        public static string CreateKey(params object[] args)
            => CreateKey((IEnumerable<string>)args);

        public static string CreateKey(IEnumerable<object> args)
        {
            var sb = new StringBuilder();
            args = args ?? Empty.ObjectArray;
            int pos = 0;
            foreach (var a in args)
            {
                var o = a;
                if (o == null || o is string)
                {
                    Stuff.Noop();
                }
                else if (o is bool)
                {
                    o = (bool)o ? 1 : 0;
                }
                else if (o.GetType().GetTypeInfo().IsEnum)
                {
                    o = Convert.ToUInt64(o);
                }
                else if (o is IEnumerable)
                {
                    o = "["+(o as IEnumerable).Format(",")+"]";
                }
                else if (o is Type)
                {
                    o = ((Type)o).FullName;
                }
                else if (o is DateTime)
                {
                    o = ((DateTime)o).ToRfc8601();
                }
                else if (o is TimeSpan)
                {
                    o = ((TimeSpan)o).TotalMilliseconds;
                }
                sb.AppendFormat("{1}`", pos, o);
                ++pos;
            }
            return CanonicalizeCacheKey(sb.ToString());
        }

        private static string CanonicalizeCacheKey(string key)
        {
            if (key == null) return "special:__NULL";
            if (key.Length < 123) return "lit:" + key;
            byte[] buf = Encoding.UTF8.GetBytes(key);
            return string.Format("urn:crc32:{0}{1}", CRC32Checksum.Do(buf), key.GetHashCode());
        }
    }
}
