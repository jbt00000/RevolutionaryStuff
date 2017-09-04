using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core;
using System.Collections.Generic;

namespace RevolutionaryStuff.SSIS
{
    public class ColumnBufferMapping
    {
        public IList<int> PositionByColumnPosition { get; }  = new List<int>();
        public IDictionary<string, int> PositionByColumnName { get; } = new Dictionary<string, int>();
        public IDictionary<string, IDtsColumn> ColumnByColumnName { get; } = new Dictionary<string, IDtsColumn>(Comparers.CaseInsensitiveStringComparer);

        public void Add(IDTSInputColumn100 column, int offset)
            => Add(new DtsColumn(column), offset);

        public void Add(IDTSOutputColumn100 column, int offset)
            => Add(new DtsColumn(column), offset);

        public void Add(IDtsColumn column, int offset)
        {
            PositionByColumnPosition.Add(offset);
            PositionByColumnName[column.Name] = offset;
            ColumnByColumnName[column.Name] = column;
        }
    }
}
