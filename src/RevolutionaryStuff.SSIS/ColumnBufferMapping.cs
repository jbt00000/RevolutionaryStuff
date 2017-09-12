using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core;
using System.Collections.Generic;

namespace RevolutionaryStuff.SSIS
{
    public class ColumnBufferMapping
    {
        private static string NormalizeColumnName(string columnName)
            => columnName.Trim().ToLower();

        public IList<int> PositionByColumnPosition { get; }  = new List<int>();

        private readonly IDictionary<string, int> PositionByColumnName = new Dictionary<string, int>(Comparers.CaseInsensitiveStringComparer);

        private readonly IDictionary<string, IDtsColumn> ColumnByColumnName = new Dictionary<string, IDtsColumn>(Comparers.CaseInsensitiveStringComparer);

        public IDtsColumn GetColumnFromColumnName(string columnName)
            => ColumnByColumnName.TryGetValue(columnName, out var z) ? z : ColumnByColumnName[NormalizeColumnName(columnName)];

        public int GetPositionFromColumnName(string columnName)
            => PositionByColumnName.TryGetValue(columnName, out var z) ? z : PositionByColumnName[NormalizeColumnName(columnName)];

        public void Add(IDTSInputColumn100 column, int offset)
            => Add(new DtsColumn(column), offset);

        public void Add(IDTSOutputColumn100 column, int offset)
            => Add(new DtsColumn(column), offset);

        public void Add(IDtsColumn column, int offset)
        {
            var name = column.Name;
            var normalizedName = name.Trim();
            PositionByColumnPosition.Add(offset);
            PositionByColumnName[name] = offset;
            PositionByColumnName[normalizedName] = offset;
            ColumnByColumnName[name] = column;
            ColumnByColumnName[normalizedName] = column;
        }
    }
}
