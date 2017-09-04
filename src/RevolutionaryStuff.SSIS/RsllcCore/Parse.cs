using System;
using System.Xml;

namespace RevolutionaryStuff.Core
{
    public static class Parse
    {
        public static T? ParseNullableEnum<T>(string s, T? fallback=null) where T : struct
        {
            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    return (T)Enum.Parse(typeof(T), s, true);
                }
                catch (Exception)
                { }
            }
            return fallback;
        }

        public static Int32? ParseNullableInt32(string s, Int32? fallback=null)
        {
            if (!string.IsNullOrEmpty(s))
            {
                Int32 i;
                if (int.TryParse(s, out i)) return i;
            }
            return fallback;
        }

        public static Int64? ParseNullableInt64(string s, Int64? fallback=null)
        {
            if (!string.IsNullOrEmpty(s))
            {
                Int64 i;
                if (long.TryParse(s, out i)) return i;
            }
            return fallback;
        }

        public static DateTime? ParseNullableDateTime(string s, DateTime? fallback = null)
        {
            if (!string.IsNullOrEmpty(s))
            {
                DateTime dt;
                if (DateTime.TryParse(s, out dt)) return dt;
            }
            return fallback;
        }

        public static DateTime ParseYYYYMMDD(string date)
        {
            date = date.Trim();
            return new DateTime(
                int.Parse(date.Substring(0, 4)),
                int.Parse(date.Substring(4, 2)),
                int.Parse(date.Substring(6, 2)));
        }

        public static double? ParseNullableDouble(string s, double? fallback=null)
        {
            if (!string.IsNullOrEmpty(s))
            {
                double i;
                if (double.TryParse(s, out i)) return i;
            }
            return fallback;
        }

        public static bool TryParseBool(string s, out bool v)
        {
            v = false;
            try
            {
                if (null != s && s.Length > 0)
                {
                    v = XmlConvert.ToBoolean(s.ToLower());
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        /// <summary>
        /// Parse an Int64.  Allows for missing value.  Empty strings return default.
        /// </summary>
        /// <param name="s">String that contains the number</param>
        /// <param name="fallback">Value to return if there is a parsing error</param>
        /// <returns>Parsed Value OR Default </returns>
        public static Int64 ParseInt64(string s, Int64 fallback=0)
        {
            Int64 ret;
            if (!Int64.TryParse(s, out ret))
            {
                ret = fallback;
            }
            return ret;
        }

        public static Uri ParseUri(string s, UriKind kind = UriKind.Absolute)
        {
            Uri u;
            return Uri.TryCreate(s, kind, out u) ? u : null;
        }

        /// <summary>
        /// Parse an double.  Allows for missing value.  Empty strings return default.
        /// </summary>
        /// <param name="s">String that contains the number</param>
        /// <param name="fallback">Value to return if there is a parsing error</param>
        /// <returns>Parsed Value OR Default </returns>
        public static double ParseDouble(string s, double fallback=0)
        {
            double ret;
            if (!double.TryParse(s, out ret))
            {
                ret = fallback;
            }
            return ret;
        }

        /// <summary>
        /// Parse an Int32.  Allows for missing value.  Empty strings return default.
        /// </summary>
        /// <param name="s">String that contains the number</param>
        /// <param name="default">Value to return if there is a parsing error</param>
        /// <returns>Parsed Value OR Default </returns>
        public static Int32 ParseInt32(string s, Int32 fallback=0)
        {
            Int32 ret;
            if (!Int32.TryParse(s, out ret))
            {
                ret = fallback;
            }
            return ret;
        }

        /// <summary>
        /// Parse an Int32.  Allows for missing value.  Empty strings return default.
        /// </summary>
        /// <param name="s">String that contains the number</param>
        /// <param name="default">Value to return if there is a parsing error</param>
        /// <returns>Parsed Value OR Default </returns>
        public static bool ParseBool(string s, bool fallback=false)
        {
            bool v;
            return Parse.TryParseBool(s, out v) ? v : fallback;
        }

        /// <summary>
        /// Parse a TimeSpan.  Allows for missing value.  
        /// </summary>
        /// <param name="s">A string that might be a timespan</param>
        /// <param name="fallback">The value to use if the conversion fails</param>
        /// <returns>Parsed value or default</returns>
        public static TimeSpan ParseTimeSpan(string s, TimeSpan fallback)
        {
            TimeSpan ts;
            if (TimeSpan.TryParse(s, out ts)) return ts;
            return fallback;
        }

        public static TEnum ParseEnum<TEnum>(string val, TEnum? fallback=null) where TEnum : struct
        {
            if (val != null && val.Length > 0)
            {
                try
                {
                    return (TEnum)Enum.Parse(typeof(TEnum), val, true);
                }
                catch (Exception)
                { }
            }
            if (fallback == null)
            {
                throw new ArgumentOutOfRangeException(nameof(val), $"{val} cannot be cast to {typeof(TEnum)}");
            }
            return fallback.Value;
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
        /// If the conversion cannot take place, use the fallback
        /// </summary>
        /// <param name="enumType">The Type of the enumeration</param>
        /// <param name="val">A string containing the name of value to convert</param>
        /// <param name="fallback">The value to use if the conversion fails</param>
        /// <returns>The converted value when possible, else fallback</returns>
        public static Enum ParseEnum(Type enumType, string val, Enum fallback)
        {
            return ParseEnum(enumType, val, true, fallback);
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
        /// If the conversion cannot take place, use the fallback
        /// </summary>
        /// <param name="enumType">The Type of the enumeration</param>
        /// <param name="val">A string containing the name of value to convert</param>
        /// <param name="ignoreCase">If true, ignore case</param>
        /// <param name="fallback">The value to use if the conversion fails</param>
        /// <returns>The converted value when possible, else fallback</returns>
        public static Enum ParseEnum(Type enumType, string val, bool ignoreCase, Enum fallback)
        {
            if (val == null || val.Length == 0) return fallback;
            try
            {
                return (Enum)Enum.Parse(enumType, val, ignoreCase);
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}
