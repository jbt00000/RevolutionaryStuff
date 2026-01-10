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

    /// <summary>
    /// Determines whether the specified date falls on a weekday (Monday through Friday).
    /// </summary>
    /// <param name="dt">The DateTime to check.</param>
    /// <returns><c>true</c> if the date is a weekday; otherwise, <c>false</c>.</returns>
    public static bool IsWeekday(this DateTime dt)
    {
        return dt.DayOfWeek switch
        {
            DayOfWeek.Saturday or DayOfWeek.Sunday => false,
            _ => true,
        };
    }

    /// <summary>
    /// Determines whether the specified date falls on a weekday (Monday through Friday).
    /// </summary>
    /// <param name="dt">The DateTimeOffset to check.</param>
    /// <returns><c>true</c> if the date is a weekday; otherwise, <c>false</c>.</returns>
    public static bool IsWeekday(this DateTimeOffset dt)
    {
        return dt.DayOfWeek switch
        {
            DayOfWeek.Saturday or DayOfWeek.Sunday => false,
            _ => true,
        };
    }

    /// <summary>
    /// Determines whether the specified date falls on a weekday (Monday through Friday).
    /// </summary>
    /// <param name="dt">The DateOnly to check.</param>
    /// <returns><c>true</c> if the date is a weekday; otherwise, <c>false</c>.</returns>
    public static bool IsWeekday(this DateOnly dt)
    {
        return dt.DayOfWeek switch
        {
            DayOfWeek.Saturday or DayOfWeek.Sunday => false,
            _ => true,
        };
    }

    /// <summary>
    /// Determines whether the specified date falls on a weekend (Saturday or Sunday).
    /// </summary>
    /// <param name="dt">The DateTime to check.</param>
    /// <returns><c>true</c> if the date is a weekend; otherwise, <c>false</c>.</returns>
    public static bool IsWeekend(this DateTime dt)
    {
        return !dt.IsWeekday();
    }

    /// <summary>
    /// Determines whether the specified date falls on a weekend (Saturday or Sunday).
    /// </summary>
    /// <param name="dt">The DateTimeOffset to check.</param>
    /// <returns><c>true</c> if the date is a weekend; otherwise, <c>false</c>.</returns>
    public static bool IsWeekend(this DateTimeOffset dt)
    {
        return !dt.IsWeekday();
    }

    /// <summary>
    /// Determines whether the specified date falls on a weekend (Saturday or Sunday).
    /// </summary>
    /// <param name="dt">The DateOnly to check.</param>
    /// <returns><c>true</c> if the date is a weekend; otherwise, <c>false</c>.</returns>
    public static bool IsWeekend(this DateOnly dt)
    {
        return !dt.IsWeekday();
    }

    /// <summary>
    /// Formats a TimeOnly as military time (24-hour format).
    /// </summary>
    /// <param name="dt">The TimeOnly to format.</param>
    /// <param name="includeSeconds">If <c>true</c>, includes seconds in the output (HH:mm:ss); otherwise, excludes seconds (HH:mm).</param>
    /// <returns>A string representing the time in military format.</returns>
    public static string ToMilitaryTime(this TimeOnly dt, bool includeSeconds = true)
        => dt.ToString(includeSeconds ? "HH:mm:ss" : "HH:mm");

    /// <summary>
    /// Formats a DateTime as military time (24-hour format).
    /// </summary>
    /// <param name="dt">The DateTime to format.</param>
    /// <param name="includeSeconds">If <c>true</c>, includes seconds in the output (HH:mm:ss); otherwise, excludes seconds (HH:mm).</param>
    /// <returns>A string representing the time in military format.</returns>
    public static string ToMilitaryTime(this DateTime dt, bool includeSeconds = true)
        => dt.ToString(includeSeconds ? "HH:mm:ss" : "HH:mm");

    /// <summary>
    /// Formats a DateTimeOffset as military time (24-hour format).
    /// </summary>
    /// <param name="dt">The DateTimeOffset to format.</param>
    /// <param name="includeSeconds">If <c>true</c>, includes seconds in the output (HH:mm:ss); otherwise, excludes seconds (HH:mm).</param>
    /// <returns>A string representing the time in military format.</returns>
    public static string ToMilitaryTime(this DateTimeOffset dt, bool includeSeconds = true)
        => dt.ToString(includeSeconds ? "HH:mm:ss" : "HH:mm");

    /// <summary>
    /// Formats a DateTime as yyyy-MM-dd.
    /// </summary>
    /// <param name="dt">The DateTime to format.</param>
    /// <returns>A string in the format yyyy-MM-dd (e.g., "2024-01-15").</returns>
    public static string ToYYYY_MM_DD(this DateTime dt)
        => dt.ToString("yyyy-MM-dd");

    /// <summary>
    /// Formats a DateTimeOffset as yyyy-MM-dd.
    /// </summary>
    /// <param name="dt">The DateTimeOffset to format.</param>
    /// <returns>A string in the format yyyy-MM-dd (e.g., "2024-01-15").</returns>
    public static string ToYYYY_MM_DD(this DateTimeOffset dt)
        => dt.ToString("yyyy-MM-dd");

    /// <summary>
    /// Formats a DateOnly as yyyy-MM-dd.
    /// </summary>
    /// <param name="dt">The DateOnly to format.</param>
    /// <returns>A string in the format yyyy-MM-dd (e.g., "2024-01-15").</returns>
    public static string ToYYYY_MM_DD(this DateOnly dt)
        => dt.ToString("yyyy-MM-dd");

    /// <summary>
    /// Formats a DateTime as yyyyMMdd (compact format without separators).
    /// </summary>
    /// <param name="dt">The DateTime to format.</param>
    /// <returns>A string in the format yyyyMMdd (e.g., "20240115").</returns>
    public static string ToYYYYMMDD(this DateTime dt)
        => dt.ToString("yyyyMMdd");

    /// <summary>
    /// Formats a DateTimeOffset as yyyyMMdd (compact format without separators).
    /// </summary>
    /// <param name="dt">The DateTimeOffset to format.</param>
    /// <returns>A string in the format yyyyMMdd (e.g., "20240115").</returns>
    public static string ToYYYYMMDD(this DateTimeOffset dt)
        => dt.ToString("yyyyMMdd");

    /// <summary>
    /// Formats a DateOnly as yyyyMMdd (compact format without separators).
    /// </summary>
    /// <param name="dt">The DateOnly to format.</param>
    /// <returns>A string in the format yyyyMMdd (e.g., "20240115").</returns>
    public static string ToYYYYMMDD(this DateOnly dt)
        => dt.ToString("yyyyMMdd");

    /// <summary>
    /// Formats a DateTime as HHmmss (compact time format without separators).
    /// </summary>
    /// <param name="dt">The DateTime to format.</param>
    /// <returns>A string in the format HHmmss (e.g., "143045" for 14:30:45).</returns>
    public static string ToHHMMSS(this DateTime dt)
        => dt.ToString("HHmmss");

    /// <summary>
    /// Formats a DateTimeOffset as HHmmss (compact time format without separators).
    /// </summary>
    /// <param name="dt">The DateTimeOffset to format.</param>
    /// <returns>A string in the format HHmmss (e.g., "143045" for 14:30:45).</returns>
    public static string ToHHMMSS(this DateTimeOffset dt)
        => dt.ToString("HHmmss");

    /// <summary>
    /// Formats a TimeOnly as HHmmss (compact time format without separators).
    /// </summary>
    /// <param name="dt">The TimeOnly to format.</param>
    /// <returns>A string in the format HHmmss (e.g., "143045" for 14:30:45).</returns>
    public static string ToHHMMSS(this TimeOnly dt)
        => dt.ToString("HHmmss");

    /// <summary>
    /// Formats a DateTime as an RFC 7231 date string.
    /// </summary>
    /// <param name="dt">The DateTime to format.</param>
    /// <returns>A string in RFC 7231 format (e.g., "Wed, 01 Oct 2008 17:04:32 GMT").</returns>
    /// <example>Wed, 01 Oct 2008 17:04:32 GMT</example>
    public static string ToRfc7231(this DateTime dt)
        => dt.ToUniversalTime().ToString("r");

    /// <summary>
    /// Formats a DateTimeOffset as an RFC 7231 date string.
    /// </summary>
    /// <param name="dt">The DateTimeOffset to format.</param>
    /// <returns>A string in RFC 7231 format (e.g., "Wed, 01 Oct 2008 17:04:32 GMT").</returns>
    /// <example>Wed, 01 Oct 2008 17:04:32 GMT</example>
    public static string ToRfc7231(this DateTimeOffset dt)
        => dt.ToUniversalTime().ToString("r");

    /// <summary>
    /// Formats a DateTime as an ISO 8601 string.
    /// </summary>
    /// <param name="dt">The DateTime to format.</param>
    /// <returns>A string in ISO 8601 format (e.g., "2008-10-01T17:04:32.0000000Z").</returns>
    /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
    /// <example>2008-10-01T17:04:32.0000000Z</example>
    public static string ToIsoString(this DateTime dt)
        => dt.ToUniversalTime().ToString("o");

    /// <summary>
    /// Formats a DateTimeOffset as an ISO 8601 string.
    /// </summary>
    /// <param name="dto">The DateTimeOffset to format.</param>
    /// <returns>A string in ISO 8601 format (e.g., "2008-10-01T17:04:32.0000000+00:00").</returns>
    /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
    /// <example>2008-10-01T17:04:32.0000000Z</example>
    public static string ToIsoString(this DateTimeOffset dto)
        => dto.ToUniversalTime().ToString("o");

    /// <summary>
    /// Formats a DateOnly as an ISO 8601 string.
    /// </summary>
    /// <param name="dt">The DateOnly to format.</param>
    /// <returns>A string in ISO 8601 date format (e.g., "2008-10-01").</returns>
    /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
    /// <example>2008-10-01</example>
    public static string ToIsoString(this DateOnly dt)
        => dt.ToString("O");

    /// <summary>
    /// Formats a TimeOnly as an ISO 8601 string.
    /// </summary>
    /// <param name="dt">The TimeOnly to format.</param>
    /// <returns>A string in ISO 8601 time format (e.g., "17:04:32.0000000").</returns>
    /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
    /// <example>17:04:32.0000000</example>
    public static string ToIsoString(this TimeOnly dt)
        => dt.ToString("O");

    /// <summary>
    /// Calculates the age in years based on a date of birth.
    /// </summary>
    /// <param name="dt">The date of birth.</param>
    /// <param name="asOf">
    /// The date to calculate the age as of. If null, uses today's date.
    /// </param>
    /// <returns>
    /// The age in completed years. Returns 0 if the date of birth is in the future.
    /// </returns>
    public static int Age(this DateTime dt, DateTime? asOf = null)
    {
        var dateTime = !asOf.HasValue ? DateTime.Today : asOf.Value;
        var age = dateTime.Year - dt.Year;
        if (dt.Month > dateTime.Month || (dt.Month == dateTime.Month && dt.Day > dateTime.Day))
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
    /// Calculates the age in years based on a date of birth.
    /// </summary>
    /// <param name="dt">The date of birth.</param>
    /// <param name="asOf">
    /// The date to calculate the age as of. If null, uses today's date.
    /// </param>
    /// <returns>
    /// The age in completed years. Returns 0 if the date of birth is in the future.
    /// </returns>
    public static int Age(this DateOnly dt, DateOnly? asOf = null)
    {
        var dateTime = !asOf.HasValue ? DateOnly.FromDateTime(DateTime.Today) : asOf.Value;
        var age = dateTime.Year - dt.Year;
        if (dt.Month > dateTime.Month || (dt.Month == dateTime.Month && dt.Day > dateTime.Day))
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
    /// Calculates the age in years based on a date of birth.
    /// </summary>
    /// <param name="dt">The date of birth.</param>
    /// <param name="asOf">
    /// The date to calculate the age as of. If null, uses today's date.
    /// </param>
    /// <returns>
    /// The age in completed years. Returns 0 if the date of birth is in the future.
    /// </returns>
    public static int Age(this DateTimeOffset dt, DateTimeOffset? asOf = null)
    {
        var dateTime = !asOf.HasValue ? DateTimeOffset.Now : asOf.Value;
        var age = dateTime.Year - dt.Year;
        if (dt.Month > dateTime.Month || (dt.Month == dateTime.Month && dt.Day > dateTime.Day))
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
    /// Converts a TimeSpan to an integer representing total milliseconds, with rounding.
    /// </summary>
    /// <param name="ts">The TimeSpan to convert.</param>
    /// <returns>The total milliseconds as an integer, rounded to the nearest value.</returns>
    public static int TotalMillisecondsToInt(this TimeSpan ts)
        => Convert.ToInt32(Math.Round(ts.TotalMilliseconds, 0));

    /// <summary>
    /// Converts a DateTime to a DateOnly, discarding the time component.
    /// </summary>
    /// <param name="dt">The DateTime to convert.</param>
    /// <returns>A DateOnly representing the date portion of the DateTime.</returns>
    public static DateOnly ToDateOnly(this DateTime dt)
        => DateOnly.FromDateTime(dt);

    /// <summary>
    /// Converts a DateTime to a TimeOnly, discarding the date component.
    /// </summary>
    /// <param name="dt">The DateTime to convert.</param>
    /// <returns>A TimeOnly representing the time portion of the DateTime.</returns>
    public static TimeOnly ToTimeOnly(this DateTime dt)
        => TimeOnly.FromDateTime(dt);

    /// <summary>
    /// Converts a DateTimeOffset to a DateOnly, discarding the time component and time zone.
    /// </summary>
    /// <param name="dt">The DateTimeOffset to convert.</param>
    /// <returns>A DateOnly representing the date portion of the DateTimeOffset.</returns>
    public static DateOnly ToDateOnly(this DateTimeOffset dt)
        => DateOnly.FromDateTime(dt.DateTime);

    /// <summary>
    /// Converts a DateTimeOffset to a TimeOnly, discarding the date component and time zone.
    /// </summary>
    /// <param name="dt">The DateTimeOffset to convert.</param>
    /// <returns>A TimeOnly representing the time portion of the DateTimeOffset.</returns>
    public static TimeOnly ToTimeOnly(this DateTimeOffset dt)
        => TimeOnly.FromDateTime(dt.DateTime);

    /// <summary>
    /// Combines a DateOnly and TimeOnly into a DateTime with the specified DateTimeKind.
    /// </summary>
    /// <param name="date">The date component.</param>
    /// <param name="time">The time component.</param>
    /// <param name="kind">The DateTimeKind to use. Defaults to Unspecified.</param>
    /// <returns>A DateTime combining the date and time.</returns>
    public static DateTime ToDateTime(this DateOnly date, TimeOnly time, DateTimeKind kind = DateTimeKind.Unspecified)
        => new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond, kind);

    /// <summary>
    /// Combines a DateOnly and TimeOnly into a DateTimeOffset with the specified offset.
    /// </summary>
    /// <param name="date">The date component.</param>
    /// <param name="time">The time component.</param>
    /// <param name="offset">The time zone offset. Defaults to UTC (zero offset).</param>
    /// <returns>A DateTimeOffset combining the date, time, and offset.</returns>
    public static DateTimeOffset ToDateTimeOffset(this DateOnly date, TimeOnly time, TimeSpan? offset = null)
        => new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond, offset ?? TimeSpan.Zero);
}
