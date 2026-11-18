namespace RevolutionaryStuff.Core;

public static class OtherExtensions
{
    extension(DateTime source)
    {
        public DateTime ToTimeZone(TimeZoneInfo zoneInfo)
                => TimeZoneInfo.ConvertTime(source, zoneInfo);
    }

    extension(short source)
    {
        public bool IsBetween(short lowerInclusive, short upperInclusive)
                => source >= lowerInclusive && source <= upperInclusive;
        public short NonZeroOr(short fallback)
        {
            return source == 0 ? fallback : source;
        }
        public short PositiveOr(short fallback)
        {
            return source < 1 ? fallback : source;
        }
    }

    extension(int source)
    {
        public bool IsBetween(int lowerInclusive, int upperInclusive)
                => source >= lowerInclusive && source <= upperInclusive;
        public bool IsOdd()
        {
            return (source & 0x1) == 1;
        }
        public bool IsEven()
        {
            return (source & 0x1) == 0;
        }
        public int NonZeroOr(int fallback)
        {
            return source == 0 ? fallback : source;
        }
        public int PositiveOr(int fallback)
        {
            return source < 1 ? fallback : source;
        }
    }

    extension(long source)
    {
        public bool IsBetween(long lowerInclusive, long upperInclusive)
                => source >= lowerInclusive && source <= upperInclusive;
        public long NonZeroOr(long fallback)
        {
            return source == 0 ? fallback : source;
        }
        public long PositiveOr(long fallback)
        {
            return source < 1 ? fallback : source;
        }
    }
















    extension(short? source)
    {
        public short PositiveOr(short fallback)
        {
            return source.GetValueOrDefault() < 1 ? fallback : source.Value;
        }
    }

    extension(int? source)
    {
        public int PositiveOr(int fallback)
        {
            return source.GetValueOrDefault() < 1 ? fallback : source.Value;
        }
    }

    extension(long? source)
    {
        public long PositiveOr(long fallback)
        {
            return source.GetValueOrDefault() < 1 ? fallback : source.Value;
        }
    }
}
