using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Crypto;
using System.Text;
using System.Collections.Generic;

namespace RevolutionaryStuff.SSIS
{
    internal class Fingerprinter
    {
        private readonly StringBuilder Strings = new StringBuilder(8000);
        private readonly SortedDictionary<string, string> ValsByKey = new SortedDictionary<string, string>();

        private readonly bool IgnoreCase;
        private readonly bool TrimThenNullifyEmptyStrings;

        public Fingerprinter(bool ignoreCase, bool trimThenNullifyEmptyStrings)
        {
            IgnoreCase = ignoreCase;
            TrimThenNullifyEmptyStrings = trimThenNullifyEmptyStrings;
        }

        public void Include(string name, object o)
        {
            name = name.Trim().ToLower();
            if (o != null && o is string)
            {
                var s = (string)o;
                s = IgnoreCase ? s.ToLower() : s;
                s = TrimThenNullifyEmptyStrings ? StringHelpers.TrimOrNull(s) : s;
                ValsByKey.Add(name, s);
            }
            else
            {
                ValsByKey.Add(name, $"{o}");
            }
            Dirty = true;
        }

        public void Clear()
        {
            ValsByKey.Clear();
            Strings.Clear();
            Dirty = true;
        }

        private bool Dirty = true;
        private string FingerPrint_p;

        public string FingerPrint
        {
            get
            {
                if (Dirty)
                {
                    foreach (var kvp in ValsByKey)
                    {
                        var sVal = (kvp.Value ?? "").ToString().Trim();
                        Strings.Append($"{kvp.Key}={sVal}`");
                    }
                    var s = Strings.ToString();
                    if (s.Length > 200)
                    {
                        var buf = Encoding.Unicode.GetBytes(s);
                        s = Hash.Compute(buf, Hash.CommonHashAlgorithmNames.Sha1).Urn;
                    }
                    FingerPrint_p = s;
                    Dirty = false;
                }
                return FingerPrint_p;
            }
        }
    }
}
