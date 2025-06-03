namespace RevolutionaryStuff.Core;

public static class DateHelpers
{
    public static DateTimeOffset UnixEarliestFileDate = new(1601, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static DateTime DateTimeFromUnixEpoch(int secondsSince1970)
        => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsSince1970);

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
        return dt.DayOfWeek switch
        {
            DayOfWeek.Saturday or DayOfWeek.Sunday => false,
            _ => true,
        };
    }

    public static bool IsWeekend(this DateTime dt)
    {
        return !dt.IsWeekday();
    }

    public static string ToMilitaryTime(this TimeOnly dt, bool includeSeconds = true)
        => dt.ToString(includeSeconds ? "HH:mm:ss" : "HH:mm");

    public static string ToMilitaryTime(this DateTime dt, bool includeSeconds = true)
        => dt.ToString(includeSeconds ? "HH:mm:ss" : "HH:mm");

    public static string ToMilitaryTime(this DateTimeOffset dt, bool includeSeconds = true)
        => dt.ToString(includeSeconds ? "HH:mm:ss" : "HH:mm");

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
    /// ISO 8601 Format
    /// </summary>
    /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
    public static string ToIsoString(this DateTime dt)
        => dt.ToUniversalTime().ToString("o");

    /// <summary>
    /// 2008-10-01T17:04:32.0000000Z
    /// ISO 8601 Format
    /// </summary>
    /// <remarks>https://en.wikipedia.org/wiki/ISO_8601</remarks>
    public static string ToIsoString(this DateTimeOffset dto)
        => dto.ToUniversalTime().ToString("o");

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

    public static int TotalMillisecondsToInt(this TimeSpan ts)
        => Convert.ToInt32(Math.Round(ts.TotalMilliseconds, 0));
}
