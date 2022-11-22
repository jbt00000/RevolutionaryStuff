namespace RevolutionaryStuff.Core;

public static class Comparers
{
    public static readonly IEqualityComparer<string> CaseInsensitiveStringComparer = new CaseInsensitiveEqualityComparer();

    private class CaseInsensitiveEqualityComparer : IEqualityComparer<string>
    {
        public CaseInsensitiveEqualityComparer()
        {
        }

        #region IEqualityComparer<string> Members

        public bool Equals(string x, string y)
        {
            return 0 == string.Compare(x, y, true);
        }

        public int GetHashCode(string obj)
        {
            return obj == null ? 0 : obj.ToLowerInvariant().GetHashCode();
        }

        #endregion
    }
}
