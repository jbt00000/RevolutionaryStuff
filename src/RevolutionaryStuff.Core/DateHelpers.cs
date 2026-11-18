namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides utility methods for working with dates, times, and time zones.
/// Includes formatting, conversion, validation, and calculation operations.
/// </summary>
public static class DateHelpers
{
    /// <summary>
    /// The earliest valid file date in Unix systems (January 1, 1601 UTC).
    /// </summary>
    public static DateTimeOffset UnixEarliestFileDate = new(1601, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Converts Unix epoch time (seconds since January 1, 1970 UTC) to a DateTime.
    /// </summary>
    /// <param name="secondsSince1970">The number of seconds since the Unix epoch.</param>
    /// <returns>A UTC DateTime representing the specified Unix time.</returns>
    public static DateTime DateTimeFromUnixEpoch(int secondsSince1970)
        => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsSince1970);

    /// <summary>
    /// Converts Unix epoch time (seconds since January 1, 1970 UTC) to a DateTimeOffset.
    /// </summary>
    /// <param name="secondsSince1970">The number of seconds since the Unix epoch.</param>
    /// <returns>A UTC DateTimeOffset representing the specified Unix time.</returns>
    public static DateTimeOffset DateTimeOffsetFromUnixEpoch(int secondsSince1970)
        => new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(secondsSince1970);

    /// <summary>
    /// Returns the first non-null DateTimeOffset from the provided values, or the earliest file date if all are null.
    /// </summary>
    /// <param name="dtos">An array of nullable DateTimeOffset values to check.</param>
    /// <returns>The first non-null value, or <see cref="UnixEarliestFileDate"/> if all values are null.</returns>
    public static DateTimeOffset GetFirstSpecifiedFileDate(params DateTimeOffset?[] dtos)
    {
        foreach (var dto in dtos)
        {
            if (dto != null) return dto.Value;
        }
        return UnixEarliestFileDate;
    }

    extension(DateTime source)
    {
        /// <summary>
        /// Determines whether the specified date falls on a weekday (Monday through Friday).
        /// </summary>
        /// <param name="source">The DateTime to check.</param>
        /// <returns><c>true</c> if the date is a weekday; otherwise, <c>false</c>.</returns>
        public bool IsWeekday()
        {
            return source.DayOfWeek switch
            {
                DayOfWeek.Saturday or DayOfWeek.Sunday => false,
                _ => true,
            };
        }
        /// <summary>
        /// Determines whether the specified date falls on a weekend (Saturday or Sunday).
        /// </summary>
        /// <param name="source">The DateTime to check.</param>
        /// <returns><c>true</c> if the date is a weekend; otherwise, <c>false</c>.</returns>
        public bool IsWeekend()
        {
            return !source.IsWeekday();
        }
        /// <summary>
        /// Formats a DateTime as military time (24-hour format).
        /// </summary>
        /// <param name="source">The DateTime to format.</param>
        /// <param name="includeSeconds">If <c>true</c>, includes seconds in the output (HH:mm:ss); otherwise, excludes seconds (HH:mm).</param>
        /// <returns>A string representing the time in military format.</returns>
        public string ToMilitaryTime(bool includeSeconds = true)
            => source.ToString(includeSeconds ? "HH:mm:ss" : "HH:mm");
        /// <summary>
        /// Formats a DateTime as yyyy-MM-dd.
        /// </summary>
        /// <param name="source">The DateTime to format.</param>
        /// <returns>A string in the format yyyy-MM-dd (e.g., "2024-01-15").</returns>
        public string ToYYYY_MM_DD()
            => source.ToString("yyyy-MM-dd");
        /// <summary>
        /// Formats a DateTime as yyyyMMdd (compact format without separators).
        /// </summary>
        /// <param name="source">The DateTime to format.</param>
        /// <returns>A string in the format yyyyMMdd (e.g., "20240115").</returns>
        public string ToYYYYMMDD()
            => source.ToString("yyyyMMdd");
        /// <summary>
        /// Formats a DateTime as HHmmss (compact time format without separators).
        /// </summary>
        /// <param name="source">The DateTime to format.</param>
        /// <returns>A string in the format HHmmss (e.g., "143045" for 14:30:45).</returns>
        public string ToHHMMSS()
            => source.ToString("HHmmss");
        /// <summary>
        /// Formats a DateTime as an RFC 7231 date string.
        /// </summary>
        /// <param name="source">The DateTime to format.</param>
        /// <returns>A string in RFC 7231 format (e.g., "Wed, 01 Oct 2008 17:04:32 GMT").</returns>
        /// <example>Wed, 01 Oct 2008 17:04:32 GMT</example>
        public string ToRfc7231()
            => source.ToUniversalTime().ToString("r");
        /// <summary>
        /// Formats a DateTime as an ISO 8601 string.
        /// </summary>
        /// <param name="source">The DateTime to format.</param>
        /// <returns>A string in ISO 8601 format (e.g., "2008-10-01T17:04:32.0000000Z").</returns>
        /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
        /// <example>2008-10-01T17:04:32.0000000Z</example>
        public string ToIsoString()
            => source.ToUniversalTime().ToString("o");
        /// <summary>
        /// Calculates the age in years based on a date of birth.
        /// </summary>
        /// <param name="source">The date of birth.</param>
        /// <param name="asOf">
        /// The date to calculate the age as of. If null, uses today's date.
        /// </param>
        /// <returns>
        /// The age in completed years. Returns 0 if the date of birth is in the future.
        /// </returns>
        public int Age(DateTime? asOf = null)
        {
            var dateTime = !asOf.HasValue ? DateTime.Today : asOf.Value;
            var age = dateTime.Year - source.Year;
            if (source.Month > dateTime.Month || (source.Month == dateTime.Month && source.Day > dateTime.Day))
            {
                --age;
            }
            if (age < 0)
            {
                age = 0;
            }
            return age;
        }
        /// <summary>
        /// Converts a DateTime to a DateOnly, discarding the time component.
        /// </summary>
        /// <param name="source">The DateTime to convert.</param>
        /// <returns>A DateOnly representing the date portion of the DateTime.</returns>
        public DateOnly ToDateOnly()
            => DateOnly.FromDateTime(source);
        /// <summary>
        /// Converts a DateTime to a TimeOnly, discarding the date component.
        /// </summary>
        /// <param name="source">The DateTime to convert.</param>
        /// <returns>A TimeOnly representing the time portion of the DateTime.</returns>
        public TimeOnly ToTimeOnly()
            => TimeOnly.FromDateTime(source);
    }

    extension(DateTimeOffset source)
    {
        /// <summary>
        /// Determines whether the specified date falls on a weekday (Monday through Friday).
        /// </summary>
        /// <param name="source">The DateTimeOffset to check.</param>
        /// <returns><c>true</c> if the date is a weekday; otherwise, <c>false</c>.</returns>
        public bool IsWeekday()
        {
            return source.DayOfWeek switch
            {
                DayOfWeek.Saturday or DayOfWeek.Sunday => false,
                _ => true,
            };
        }
        /// <summary>
        /// Determines whether the specified date falls on a weekend (Saturday or Sunday).
        /// </summary>
        /// <param name="source">The DateTimeOffset to check.</param>
        /// <returns><c>true</c> if the date is a weekend; otherwise, <c>false</c>.</returns>
        public bool IsWeekend()
        {
            return !source.IsWeekday();
        }
        /// <summary>
        /// Formats a DateTimeOffset as military time (24-hour format).
        /// </summary>
        /// <param name="source">The DateTimeOffset to format.</param>
        /// <param name="includeSeconds">If <c>true</c>, includes seconds in the output (HH:mm:ss); otherwise, excludes seconds (HH:mm).</param>
        /// <returns>A string representing the time in military format.</returns>
        public string ToMilitaryTime(bool includeSeconds = true)
            => source.ToString(includeSeconds ? "HH:mm:ss" : "HH:mm");
        /// <summary>
        /// Formats a DateTimeOffset as yyyy-MM-dd.
        /// </summary>
        /// <param name="source">The DateTimeOffset to format.</param>
        /// <returns>A string in the format yyyy-MM-dd (e.g., "2024-01-15").</returns>
        public string ToYYYY_MM_DD()
            => source.ToString("yyyy-MM-dd");
        /// <summary>
        /// Formats a DateTimeOffset as yyyyMMdd (compact format without separators).
        /// </summary>
        /// <param name="source">The DateTimeOffset to format.</param>
        /// <returns>A string in the format yyyyMMdd (e.g., "20240115").</returns>
        public string ToYYYYMMDD()
            => source.ToString("yyyyMMdd");
        /// <summary>
        /// Formats a DateTimeOffset as HHmmss (compact time format without separators).
        /// </summary>
        /// <param name="source">The DateTimeOffset to format.</param>
        /// <returns>A string in the format HHmmss (e.g., "143045" for 14:30:45).</returns>
        public string ToHHMMSS()
            => source.ToString("HHmmss");
        /// <summary>
        /// Formats a DateTimeOffset as an RFC 7231 date string.
        /// </summary>
        /// <param name="source">The DateTimeOffset to format.</param>
        /// <returns>A string in RFC 7231 format (e.g., "Wed, 01 Oct 2008 17:04:32 GMT").</returns>
        /// <example>Wed, 01 Oct 2008 17:04:32 GMT</example>
        public string ToRfc7231()
            => source.ToUniversalTime().ToString("r");
        /// <summary>
        /// Formats a DateTimeOffset as an ISO 8601 string.
        /// </summary>
        /// <param name="source">The DateTimeOffset to format.</param>
        /// <returns>A string in ISO 8601 format (e.g., "2008-10-01T17:04:32.0000000+00:00").</returns>
        /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
        /// <example>2008-10-01T17:04:32.0000000Z</example>
        public string ToIsoString()
            => source.ToUniversalTime().ToString("o");
        /// <summary>
        /// Calculates the age in years based on a date of birth.
        /// </summary>
        /// <param name="source">The date of birth.</param>
        /// <param name="asOf">
        /// The date to calculate the age as of. If null, uses today's date.
        /// </param>
        /// <returns>
        /// The age in completed years. Returns 0 if the date of birth is in the future.
        /// </returns>
        public int Age(DateTimeOffset? asOf = null)
        {
            var dateTime = !asOf.HasValue ? DateTimeOffset.Now : asOf.Value;
            var age = dateTime.Year - source.Year;
            if (source.Month > dateTime.Month || (source.Month == dateTime.Month && source.Day > dateTime.Day))
            {
                --age;
            }
            if (age < 0)
            {
                age = 0;
            }
            return age;
        }
        /// <summary>
        /// Converts a DateTimeOffset to a DateOnly, discarding the time component and time zone.
        /// </summary>
        /// <param name="source">The DateTimeOffset to convert.</param>
        /// <returns>A DateOnly representing the date portion of the DateTimeOffset.</returns>
        public DateOnly ToDateOnly()
            => DateOnly.FromDateTime(source.DateTime);
        /// <summary>
        /// Converts a DateTimeOffset to a TimeOnly, discarding the date component and time zone.
        /// </summary>
        /// <param name="source">The DateTimeOffset to convert.</param>
        /// <returns>A TimeOnly representing the time portion of the DateTimeOffset.</returns>
        public TimeOnly ToTimeOnly()
            => TimeOnly.FromDateTime(source.DateTime);
    }

    extension(DateOnly source)
    {
        /// <summary>
        /// Determines whether the specified date falls on a weekday (Monday through Friday).
        /// </summary>
        /// <param name="source">The DateOnly to check.</param>
        /// <returns><c>true</c> if the date is a weekday; otherwise, <c>false</c>.</returns>
        public bool IsWeekday()
        {
            return source.DayOfWeek switch
            {
                DayOfWeek.Saturday or DayOfWeek.Sunday => false,
                _ => true,
            };
        }
        /// <summary>
        /// Determines whether the specified date falls on a weekend (Saturday or Sunday).
        /// </summary>
        /// <param name="source">The DateOnly to check.</param>
        /// <returns><c>true</c> if the date is a weekend; otherwise, <c>false</c>.</returns>
        public bool IsWeekend()
        {
            return !source.IsWeekday();
        }
        /// <summary>
        /// Formats a DateOnly as yyyy-MM-dd.
        /// </summary>
        /// <param name="source">The DateOnly to format.</param>
        /// <returns>A string in the format yyyy-MM-dd (e.g., "2024-01-15").</returns>
        public string ToYYYY_MM_DD()
            => source.ToString("yyyy-MM-dd");
        /// <summary>
        /// Formats a DateOnly as yyyyMMdd (compact format without separators).
        /// </summary>
        /// <param name="source">The DateOnly to format.</param>
        /// <returns>A string in the format yyyyMMdd (e.g., "20240115").</returns>
        public string ToYYYYMMDD()
            => source.ToString("yyyyMMdd");
        /// <summary>
        /// Formats a DateOnly as an ISO 8601 string.
        /// </summary>
        /// <param name="source">The DateOnly to format.</param>
        /// <returns>A string in ISO 8601 date format (e.g., "2008-10-01").</returns>
        /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
        /// <example>2008-10-01</example>
        public string ToIsoString()
            => source.ToString("O");
        /// <summary>
        /// Calculates the age in years based on a date of birth.
        /// </summary>
        /// <param name="source">The date of birth.</param>
        /// <param name="asOf">
        /// The date to calculate the age as of. If null, uses today's date.
        /// </param>
        /// <returns>
        /// The age in completed years. Returns 0 if the date of birth is in the future.
        /// </returns>
        public int Age(DateOnly? asOf = null)
        {
            var dateTime = !asOf.HasValue ? DateOnly.FromDateTime(DateTime.Today) : asOf.Value;
            var age = dateTime.Year - source.Year;
            if (source.Month > dateTime.Month || (source.Month == dateTime.Month && source.Day > dateTime.Day))
            {
                --age;
            }
            if (age < 0)
            {
                age = 0;
            }
            return age;
        }
        /// <summary>
        /// Combines a DateOnly and TimeOnly into a DateTime with the specified DateTimeKind.
        /// </summary>
        /// <param name="source">The date component.</param>
        /// <param name="time">The time component.</param>
        /// <param name="kind">The DateTimeKind to use. Defaults to Unspecified.</param>
        /// <returns>A DateTime combining the date and time.</returns>
        public DateTime ToDateTime(TimeOnly time, DateTimeKind kind = DateTimeKind.Unspecified)
            => new(source.Year, source.Month, source.Day, time.Hour, time.Minute, time.Second, time.Millisecond, kind);
        /// <summary>
        /// Combines a DateOnly and TimeOnly into a DateTimeOffset with the specified offset.
        /// </summary>
        /// <param name="source">The date component.</param>
        /// <param name="time">The time component.</param>
        /// <param name="offset">The time zone offset. Defaults to UTC (zero offset).</param>
        /// <returns>A DateTimeOffset combining the date, time, and offset.</returns>
        public DateTimeOffset ToDateTimeOffset(TimeOnly time, TimeSpan? offset = null)
            => new(source.Year, source.Month, source.Day, time.Hour, time.Minute, time.Second, time.Millisecond, offset ?? TimeSpan.Zero);
    }







    extension(TimeOnly source)
    {
        /// <summary>
        /// Formats a TimeOnly as military time (24-hour format).
        /// </summary>
        /// <param name="source">The TimeOnly to format.</param>
        /// <param name="includeSeconds">If <c>true</c>, includes seconds in the output (HH:mm:ss); otherwise, excludes seconds (HH:mm).</param>
        /// <returns>A string representing the time in military format.</returns>
        public string ToMilitaryTime(bool includeSeconds = true)
            => source.ToString(includeSeconds ? "HH:mm:ss" : "HH:mm");
        /// <summary>
        /// Formats a TimeOnly as HHmmss (compact time format without separators).
        /// </summary>
        /// <param name="source">The TimeOnly to format.</param>
        /// <returns>A string in the format HHmmss (e.g., "143045" for 14:30:45).</returns>
        public string ToHHMMSS()
            => source.ToString("HHmmss");
        /// <summary>
        /// Formats a TimeOnly as an ISO 8601 string.
        /// </summary>
        /// <param name="source">The TimeOnly to format.</param>
        /// <returns>A string in ISO 8601 time format (e.g., "17:04:32.0000000").</returns>
        /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
        /// <example>17:04:32.0000000</example>
        public string ToIsoString()
            => source.ToString("O");
    }









































    extension(TimeSpan source)
    {
        /// <summary>
        /// Converts a TimeSpan to an integer representing total milliseconds, with rounding.
        /// </summary>
        /// <param name="source">The TimeSpan to convert.</param>
        /// <returns>The total milliseconds as an integer, rounded to the nearest value.</returns>
        public int TotalMillisecondsToInt()
            => Convert.ToInt32(Math.Round(source.TotalMilliseconds, 0));
    }












}
