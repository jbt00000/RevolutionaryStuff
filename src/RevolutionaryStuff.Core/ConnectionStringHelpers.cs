namespace RevolutionaryStuff.Core;

public static class ConnectionStringHelpers
{
    public static IDictionary<string, string> ConnectionStringToDictionary(string connectionString, string pairSeparator = ";", string keyValueSeparator = "=", bool keysAreCaseSensitive = false)
    {
        var d = keysAreCaseSensitive ? new Dictionary<string, string>() : new Dictionary<string, string>(Comparers.CaseInsensitiveStringComparer);
        var pairs = (connectionString ?? "").Split(';');
        foreach (var pair in pairs)
        {
            StringHelpers.Split(pair, keyValueSeparator, true, out string left, out string right);
            d[(left ?? "").Trim()] = StringHelpers.TrimOrNull(right);
        }
        return d;
    }

    public static string GetValue(string connectionString, string key, string fallback = null)
        => ConnectionStringToDictionary(connectionString).GetValue(key, fallback);
}
