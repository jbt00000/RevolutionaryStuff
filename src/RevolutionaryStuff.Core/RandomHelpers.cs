using System.Text;

namespace RevolutionaryStuff.Core;

public static class RandomHelpers
{
    public static string NextString(this Random r, int characterCount, string characterSet)
    {
        Requires.NonNegative(characterCount);
        Requires.Text(characterSet);

        var sb = new StringBuilder(characterCount);
        for (var z = 0; z < characterCount; ++z)
        {
            var i = r.Next(characterSet.Length);
            var ch = characterSet[i];
            sb.Append(ch);
        }
        return sb.ToString();
    }

    public static bool NextBoolean(this Random r)
        => r.Next(2) == 1;
}
