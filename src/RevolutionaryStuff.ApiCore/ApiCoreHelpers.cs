namespace RevolutionaryStuff.ApiCore;

public static class ApiCoreHelpers
{
    public static class CommonCultureNames
    {
        public const string EnUs = "en-US";
    }

    public static class CommonCultureInfos
    {
        public static readonly System.Globalization.CultureInfo EnUs = new(CommonCultureNames.EnUs);
    }
}
