namespace RevolutionaryStuff.Core;

public static class RandomHelpers
{
    public static string NextString(this Random r, int characterCount, string characterSet)
    {
        var s = "";
        for (var z = 0; z < characterCount; ++z)
        {
            var i = r.Next(characterSet.Length);
            var ch = characterSet[i];
            s += ch;
        }
        return s;
    }

    public static bool NextBoolean(this Random r)
        => r.Next(2) == 1;
}
