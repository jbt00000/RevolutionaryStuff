using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RevolutionaryStuff.Core.Collections
{
    internal class Grouping<K, V> : IGrouping<K, V>
    {
        private readonly K Key;
        private readonly IEnumerable<V> Vals;

        public Grouping(K key, IEnumerable<V> vals)
        {
            Key = key;
            Vals = vals;
        }

        #region IGrouping<K,V> Members

        K IGrouping<K, V>.Key
        {
            [DebuggerStepThrough]
            get { return Key; }
        }

        #endregion

        #region IEnumerable<V> Members

        IEnumerator<V> IEnumerable<V>.GetEnumerator()
        {
            return Vals.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (object o in Vals)
            {
                yield return o;
            }
        }

        #endregion
    }
}
