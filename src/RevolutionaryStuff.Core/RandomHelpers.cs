using System.Text;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides helpers for generating values with a <see cref="Random"/> instance.
/// </summary>
public static class RandomHelpers
{
    /// <summary>
    /// Generates a string by selecting characters from a supplied character set.
    /// </summary>
    /// <param name="r">The random number generator to use.</param>
    /// <param name="characterCount">The number of characters to generate.</param>
    /// <param name="characterSet">The characters from which each output character is selected.</param>
    /// <returns>A string containing <paramref name="characterCount"/> randomly selected characters.</returns>
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

    /// <summary>
    /// Generates a random boolean value.
    /// </summary>
    /// <param name="r">The random number generator to use.</param>
    /// <returns>A randomly selected <see langword="true"/> or <see langword="false"/> value.</returns>
    public static bool NextBoolean(this Random r)
        => r.Next(2) == 1;
}
