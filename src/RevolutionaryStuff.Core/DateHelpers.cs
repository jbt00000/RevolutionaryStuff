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
    }
}
