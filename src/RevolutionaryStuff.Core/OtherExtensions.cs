﻿using System;

namespace RevolutionaryStuff.Core
{
    public static class OtherExtensions
    {
        public static DateTime ToTimeZone(this DateTime dt, TimeZoneInfo zoneInfo)
            => TimeZoneInfo.ConvertTime(dt, zoneInfo);

        public static bool IsWeekday(this DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                    return false;
                default:
                    return true;
            }
        }

        public static bool IsWeekend(this DateTime dt)
        {
            return !dt.IsWeekday();
        }

        public static string ToYYYYMMDD(this DateTime dt)
            => dt.ToString("yyyyMMdd");

        public static string ToHHMMSS(this DateTime dt)
            => dt.ToString("HHmmss");

        /// <summary>
        /// Wed, 01 Oct 2008 17:04:32 GMT
        /// </summary>
        public static string ToRfc7231(this DateTime dt)
            => dt.ToUniversalTime().ToString("r");

        /// <summary>
        /// 2008-10-01T17:04:32.0000000Z
        /// </summary>
        public static string ToRfc8601(this DateTime dt)
            => dt.ToUniversalTime().ToString("o") + "Z";

        public static int Age(this DateTime dt)
        {
            DateTime now = DateTime.Now;
            int age = now.Year - dt.Year;
            age += now.DayOfYear < dt.DayOfYear ? -1 : 0;
            if (age < 0) age = 0;
            return age;
        }

        public static bool IsBetween(this short n, short lowerInclusive, short upperInclusive)
            => n >= lowerInclusive && n <= upperInclusive;

        public static bool IsBetween(this int n, int lowerInclusive, int upperInclusive)
            => n >= lowerInclusive && n <= upperInclusive;

        public static bool IsBetween(this long n, long lowerInclusive, long upperInclusive)
            => n >= lowerInclusive && n <= upperInclusive;

        public static bool IsOdd(this Int32 i)
        {
            return ((i & 0x1) == 1);
        }
        public static bool IsEven(this Int32 i)
        {
            return ((i & 0x1) == 0);
        }

        public static Int16 NonZeroOr(this Int16 i, Int16 fallback)
        {
            if (i == 0) return fallback;
            return i;
        }

        public static Int32 NonZeroOr(this Int32 i, Int32 fallback)
        {
            if (i == 0) return fallback;
            return i;
        }

        public static Int64 NonZeroOr(this Int64 i, Int64 fallback)
        {
            if (i == 0) return fallback;
            return i;
        }

        public static Int16 PositiveOr(this Int16 i, Int16 fallback)
        {
            if (i < 1) return fallback;
            return i;
        }

        public static Int32 PositiveOr(this Int32 i, Int32 fallback)
        {
            if (i < 1) return fallback;
            return i;
        }

        public static Int64 PositiveOr(this Int64 i, Int64 fallback)
        {
            if (i < 1) return fallback;
            return i;
        }

        public static Int16 PositiveOr(this Int16? i, Int16 fallback)
        {
            if (i.GetValueOrDefault() < 1) return fallback;
            return i.Value;
        }

        public static Int32 PositiveOr(this Int32? i, Int32 fallback)
        {
            if (i.GetValueOrDefault() < 1) return fallback;
            return i.Value;
        }

        public static Int64 PositiveOr(this Int64? i, Int64 fallback)
        {
            if (i.GetValueOrDefault() < 1) return fallback;
            return i.Value;
        }
    }
}
