namespace RevolutionaryStuff.Core;

public static class ConnectionStringHelpers
{
    public static IDictionary<string, string> ConnectionStringToDictionary(string connectionString, string pairSeparator = ";", string keyValueSeparator = "=", bool keysAreCaseSensitive = false)
    {
        var d = keysAreCaseSensitive ? new Dictionary<string, string>() : new Dictionary<string, string>(Comparers.CaseInsensitiveStringComparer);
        var pairs = (connectionString ?? "").Split(';');
        foreach (var pair in pairs)
        {
            pair.Split(keyValueSeparator, true, out var left, out var right);
            d[(left ?? "").Trim()] = right.TrimOrNull();
        }
        return d;
    }

    public static string GetValue(string connectionString, string key, string fallback = null)
        => ConnectionStringToDictionary(connectionString).GetValue(key, fallback);
}
