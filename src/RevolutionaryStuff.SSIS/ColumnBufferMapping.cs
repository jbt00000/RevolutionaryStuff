using System.Collections.Generic;

namespace RevolutionaryStuff.SSIS
{
    public class ColumnBufferMapping
    {
        public IList<int> ByColumnPosition = new List<int>();
        public IDictionary<string, int> ByColumnName = new Dictionary<string, int>();

        public void Add(string columnName, int offset)
        {
            ByColumnPosition.Add(offset);
            ByColumnName[columnName] = offset;
        }
    }
}
