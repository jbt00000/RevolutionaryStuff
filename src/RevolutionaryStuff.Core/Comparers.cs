using System.Collections.Generic;

namespace RevolutionaryStuff.Core
{
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
                if (obj == null) return 0;
                return obj.ToLowerInvariant().GetHashCode();
            }

            #endregion
        }
    }
}
