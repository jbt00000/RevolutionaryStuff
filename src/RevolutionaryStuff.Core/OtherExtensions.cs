using System;

namespace RevolutionaryStuff.Core
{
    public static class OtherExtensions
    {
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

        public static string ToRfc7231(this DateTime dt) 
            => dt.ToUniversalTime().ToString("r");

        public static int Age(this DateTime dt)
        {
            DateTime now = DateTime.Now;
            int age = now.Year - dt.Year;
            age += now.DayOfYear < dt.DayOfYear ? -1 : 0;
            if (age < 0) age = 0;
            return age;
        }

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
