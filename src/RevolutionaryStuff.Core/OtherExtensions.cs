namespace RevolutionaryStuff.Core;
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

    public static bool IsOdd(this int i)
    {
        return (i & 0x1) == 1;
    }
    public static bool IsEven(this int i)
    {
        return (i & 0x1) == 0;
    }

    public static short NonZeroOr(this short i, short fallback)
    {
        return i == 0 ? fallback : i;
    }

    public static int NonZeroOr(this int i, int fallback)
    {
        return i == 0 ? fallback : i;
    }

    public static long NonZeroOr(this long i, long fallback)
    {
        return i == 0 ? fallback : i;
    }

    public static short PositiveOr(this short i, short fallback)
    {
        return i < 1 ? fallback : i;
    }

    public static int PositiveOr(this int i, int fallback)
    {
        return i < 1 ? fallback : i;
    }

    public static long PositiveOr(this long i, long fallback)
    {
        return i < 1 ? fallback : i;
    }

    public static short PositiveOr(this short? i, short fallback)
    {
        return i.GetValueOrDefault() < 1 ? fallback : i.Value;
    }

    public static int PositiveOr(this int? i, int fallback)
    {
        return i.GetValueOrDefault() < 1 ? fallback : i.Value;
    }

    public static long PositiveOr(this long? i, long fallback)
    {
        return i.GetValueOrDefault() < 1 ? fallback : i.Value;
    }
}
