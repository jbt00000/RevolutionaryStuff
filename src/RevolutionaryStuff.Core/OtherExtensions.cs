using System;

namespace RevolutionaryStuff.Core
{
    public static class OtherExtensions
    {
        public static DateTime ToTimeZone(this DateTime dt, TimeZoneInfo zoneInfo)
            => TimeZoneInfo.ConvertTime(dt, zoneInfo);

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
