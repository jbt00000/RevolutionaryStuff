using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RevolutionaryStuff.SSIS
{
    public class ColumnBufferMapping
    {
        private static int ID_s = 1;

        public readonly string ID = $"CMB{Interlocked.Increment(ref ID_s)}Z";

        private static string NormalizeColumnName(string columnName)
            => columnName.Trim().ToLower();

        public IList<int> PositionByColumnPosition { get; }  = new List<int>();

        private readonly IDictionary<string, int> PositionByColumnName = new Dictionary<string, int>(Comparers.CaseInsensitiveStringComparer);

        private readonly IList<string> ColumnNameByPosition = new List<string>();

        private readonly IDictionary<string, IDtsColumn> ColumnByColumnName = new Dictionary<string, IDtsColumn>(Comparers.CaseInsensitiveStringComparer);

        public int ColumnCount { get; private set; }

        public bool ColumnExists(string columnName)
            => ColumnByColumnName.ContainsKey(columnName);

        public IDtsColumn GetColumnFromColumnName(string columnName)
        {
            try
            {
                return ColumnByColumnName.TryGetValue(columnName, out var z) ? z : ColumnByColumnName[NormalizeColumnName(columnName)];
            }
            catch (Exception ex)
            {
                throw new Exception($"Problem fetching column [{columnName}] in cbm.ID={this.ID}", ex);
            }
        }

        public int GetPositionFromColumnName(string columnName)
        {
            try
            {
                return PositionByColumnName.TryGetValue(columnName, out var z) ? z : PositionByColumnName[NormalizeColumnName(columnName)];
            }
            catch (Exception ex)
            {
                throw new Exception($"Problem fetching column position for [{columnName}] in cbm.ID={this.ID}", ex);
            }
        }

        public string GetColumnNameFromPosition(int pos)
            => ColumnNameByPosition[pos];

        public void Add(IDTSInputColumn100 column, int offset)
            => Add(new DtsColumn(column), offset);

        public void Add(IDTSOutputColumn100 column, int offset)
            => Add(new DtsColumn(column), offset);

        public void Add(IDtsColumn column, int offset)
        {
            var name = column.Name;
            var normalizedName = name.Trim();
            ColumnNameByPosition.Add(name);
            PositionByColumnPosition.Add(offset);
            PositionByColumnName[name] = offset;
            PositionByColumnName[normalizedName] = offset;
            ColumnByColumnName[name] = column;
            ColumnByColumnName[normalizedName] = column;
            ++ColumnCount;
        }
    }
}
