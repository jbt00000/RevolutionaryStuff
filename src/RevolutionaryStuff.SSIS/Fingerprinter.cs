using RevolutionaryStuff.Core.Crypto;
using System.Text;
using System.Collections.Generic;

namespace RevolutionaryStuff.SSIS
{
    internal class Fingerprinter
    {
        private readonly StringBuilder Strings = new StringBuilder(8000);
        private readonly SortedDictionary<string, object> ValsByKey = new SortedDictionary<string, object>();

        public void Include(string name, object o)
        {
            ValsByKey.Add(name, o);
        }

        public void Clear()
        {
            ValsByKey.Clear();
            Strings.Clear();
        }

        public string GetFingerPrint()
        {
            foreach (var kvp in ValsByKey)
            {
                var sVal = (kvp.Value ?? "").ToString().Trim();
                Strings.Append($"{kvp.Key.ToLower()}={sVal}|");
            }
            var s = Strings.ToString();
            if (s.Length > 200)
            {
                var buf = Encoding.Unicode.GetBytes(s);
                s = Hash.Compute(buf, Hash.CommonHashAlgorithmNames.Sha1).Urn;
            }
            return "[" + s + "]";
        }
    }
}
