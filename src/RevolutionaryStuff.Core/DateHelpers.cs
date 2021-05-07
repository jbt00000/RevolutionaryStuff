using System;

namespace RevolutionaryStuff.Core
{
    public static class DateHelpers
    {
        public static DateTimeOffset UnixEarliestFileDate = new DateTimeOffset(1601, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static DateTimeOffset GetFirstSpecifiedFileDate(params DateTimeOffset?[] dtos)
        {
            foreach (var dto in dtos)
            {
                if (dto != null) return dto.Value;
            }
            return UnixEarliestFileDate;
        }

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

        public static string ToYYYY_MM_DD(this DateTime dt)
            => dt.ToString("yyyy-MM-dd");

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
        [Obsolete("Use ToIsoString instead", false)]
        public static string ToRfc8601(this DateTime dt)
            => dt.ToIsoString();

        /// <summary>
        /// 2008-10-01T17:04:32.0000000Z
        /// ISO 8601 Format
        /// </summary>
        /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
        public static string ToIsoString(this DateTime dt)
        {
            var s = dt.ToUniversalTime().ToString("o");
            if (!s.EndsWith("Z"))
            {
                s += "Z";
            }
            return s;
        }

        /// <summary>
        /// 2008-10-01T17:04:32.0000000Z
        /// </summary>
        [Obsolete("Use ToIsoString instead", false)]
        public static string ToRfc8601(this DateTimeOffset dto)
            => dto.UtcDateTime.ToIsoString();

        /// <summary>
        /// 2008-10-01T17:04:32.0000000Z
        /// ISO 8601 Format
        /// </summary>
        /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
        public static string ToIsoString(this DateTimeOffset dto)
            => dto.ToIsoString();

        public static int Age(this DateTime dt)
        {
            var now = DateTime.Today;
            int age = now.Year - dt.Year;
            if (dt.Month > now.Month || (dt.Month == now.Month && dt.Day > now.Day))
            {
                age -= 1;
            }
            if (age < 0) age = 0;
            return age;
        }
    }
}
