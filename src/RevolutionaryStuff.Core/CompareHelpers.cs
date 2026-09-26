namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides helpers for comparing byte arrays and checking array membership.
/// </summary>
public static class CompareHelpers
{
    /// <summary>
    /// Determines whether two byte arrays contain the same bytes in the same order.
    /// </summary>
    /// <param name="b1">The first byte array.</param>
    /// <param name="b2">The second byte array.</param>
    /// <returns><see langword="true"/> if both references are equal or the arrays have identical contents; otherwise, <see langword="false"/>.</returns>
    public static bool Compare(byte[] b1, byte[] b2)
    {
        if (b1 == b2) return true;
        if (b1 == null || b2 == null || b1.Length != b2.Length) return false;
        var len = b1.Length;
        for (var x = 0; x < len; ++x)
        {
            if (b1[x] != b2[x])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Determines whether an array contains a specified value.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="items">The array to search.</param>
    /// <param name="test">The value to find.</param>
    /// <returns><see langword="true"/> if the array contains <paramref name="test"/>; otherwise, <see langword="false"/>.</returns>
    public static bool Contains<T>(this T[] items, T test) => ((ICollection<T>)items).Contains(test);
}
