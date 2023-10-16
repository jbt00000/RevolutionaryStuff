﻿using System.Text.Json;

namespace RevolutionaryStuff.Core;
public static partial class JsonHelpers
{
    public static string GetString(this IDictionary<string, JsonElement> extensionData, string key, string missing = default)
        => extensionData != null && key != null && extensionData.TryGetValue(key, out var je) ? je.GetString() : missing;

    public static int GetInt(this IDictionary<string, JsonElement> extensionData, string key, int missing = default)
        => extensionData != null && key != null && extensionData.TryGetValue(key, out var je) ? je.GetInt32() : missing;
}
