using RevolutionaryStuff.Core.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace RevolutionaryStuff.Core.Data
{
    public class SimpleDataSet : IDataSet
    {
        public string Name { get; set; }
        public IList<IDataTable> Tables { get; } = new List<IDataTable>();
    }

    public class SimpleDataTable : IDataTable
    {
        public string TableName { get; set; }
        public IList<IDataColumn> Columns { get; private set; }
        public IDataRowCollection Rows { get; private set; }

        public SimpleDataTable()
        {
            var columns = new NotifiedList<IDataColumn>();
            Columns = columns;
            columns.Added += delegate (object sender, EventArgs<IEnumerable<IDataColumn>> ea)
            {
                foreach (var col in ea.Data)
                {
                    Requires.Null(col.Table, $"column name=[{col.ColumnName}]");
                    var scol = col as SimpleDataColumn;
                    if (scol != null)
                    {
                        scol.Table = this;
                    }
                }
            };
            columns.Removed += delegate (object sender, EventArgs<IEnumerable<IDataColumn>> ea)
            {
                foreach (var scol in ea.Data.OfType<SimpleDataColumn>())
                {
                    scol.Table = null;
                }
            };
            columns.Changed += delegate (object sender, EventArgs e)
            {
                foreach (var scol in columns.OfType<SimpleDataColumn>())
                {
                    scol.Ordinal_p = -1;
                }
                lock (ColumnPositionByColumnName)
                {
                    ColumnPositionByColumnName.Clear();
                }
            };
            Rows = new SimpleDataRowCollection(this);
        }

        private class SimpleDataRowCollection : IDataRowCollection
        {
            private readonly SimpleDataTable Table;
            private readonly IList<IDataRow> Rows;

            int IDataRowCollection.Count
            {
                get
                {
                    return Rows.Count;
                }
            }

            IDataRow IDataRowCollection.this[int i]
            {
                get
                {
                    return Rows[i];
                }
            }

            void IDataRowCollection.Add(object[] fields)
            {
                var row = new SimpleDataRow(Table, fields);
                Add(row);
            }

            public void Add(IDataRow dataRow)
            {
                Requires.NonNull(dataRow, nameof(dataRow));
                Rows.Add(dataRow);
            }

            public SimpleDataRowCollection(SimpleDataTable table)
            {
                Requires.NonNull(table, nameof(table));
                Table = table;

                var rows = new NotifiedList<IDataRow>();
                Rows = rows;
                rows.Added += delegate (object sender, EventArgs<IEnumerable<IDataRow>> ea)
                {
                    foreach (var row in ea.Data)
                    {
                        if (row.Table != table)
                        {
                            Requires.Null(row.Table, "row.Table");
                        }
                        var srow = row as SimpleDataRow;
                        if (srow != null)
                        {
                            srow.Table = Table;
                        }
                    }
                };
                rows.Removed += delegate (object sender, EventArgs<IEnumerable<IDataRow>> ea)
                {
                    foreach (var srow in ea.Data.OfType<SimpleDataRow>())
                    {
                        srow.Table = null;
                    }
                };
            }
        }

        public IDataRow NewRow()
        {
            return new SimpleDataRow(this);
        }

        private IDictionary<string, int> ColumnPositionByColumnName = new Dictionary<string, int>(Comparers.CaseInsensitiveStringComparer);

        private int GetColumnPosition(string columnName)
        {
            if (columnName != null)
            {
                lock (ColumnPositionByColumnName)
                {
                    if (ColumnPositionByColumnName.Count == 0)
                    {
                        this.Columns.ForEach(c => ColumnPositionByColumnName[c.ColumnName] = ColumnPositionByColumnName.Count);
                    }
                    int pos;
                    if (ColumnPositionByColumnName.TryGetValue(columnName, out pos)) return pos;
                }
            }
            return -1;
        }

        private class SimpleDataRow : IDataRow
        {
            IDataTable IDataRow.Table
            {
                get
                {
                    return Table;
                }
            }

            public SimpleDataTable Table { get; internal set; }

            public int FieldCount
            {
                get
                {
                    return this.Cells.Length;
                }
            }

            public object this[string name]
            {
                get { return this[Table.GetColumnPosition(name)]; }
                set { this[Table.GetColumnPosition(name)] = value; }
            }

            public object this[int i]
            {
                get
                {
                    Requires.Between(i, nameof(i), 0, Table.Columns.Count);
                    if (i > Cells.Length) return null;
                    return Cells[i];
                }
                set
                {
                    Requires.Between(i, nameof(i), 0, Table.Columns.Count);
                    if (i > Cells.Length)
                    {
                        Array.Resize(ref Cells, Table.Columns.Count);
                    }
                    Cells[i] = value;
                }
            }

            private object[] Cells;

            public SimpleDataRow(SimpleDataTable table, object[] vals = null)
            {
                Requires.NonNull(table, nameof(table));
                Table = table;
                if (vals != null && vals.Length > Table.Columns.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(vals), "has too many vals and thus does not fit table definition");
                }
                Cells = vals ?? new object[Table.Columns.Count];
            }

            public bool GetBoolean(int i)
            {
                return (bool)this[i];
            }

            public byte GetByte(int i)
            {
                return (byte)this[i];
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public char GetChar(int i)
            {
                return (char)this[i];
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public IDataReader GetData(int i)
            {
                throw new NotImplementedException();
            }

            public string GetDataTypeName(int i)
            {
                return Table.Columns[i].DataType.Name;
            }

            public DateTime GetDateTime(int i)
            {
                return (DateTime)this[i];
            }

            public decimal GetDecimal(int i)
            {
                return (decimal)this[i];
            }

            public double GetDouble(int i)
            {
                return (double)this[i];
            }

            public Type GetFieldType(int i)
            {
                return Table.Columns[i].DataType;
            }

            public float GetFloat(int i)
            {
                return (float)this[i];
            }

            public Guid GetGuid(int i)
            {
                return (Guid)this[i];
            }

            public short GetInt16(int i)
            {
                return (short)this[i];
            }

            public int GetInt32(int i)
            {
                return (int)this[i];
            }

            public long GetInt64(int i)
            {
                return (long)this[i];
            }

            public string GetName(int i)
            {
                return Table.Columns[i].ColumnName;
            }

            public int GetOrdinal(string name)
            {
                return Table.GetColumnPosition(name);
            }

            public string GetString(int i)
            {
                return (string)this[i];
            }

            public object GetValue(int i)
            {
                return this[i];
            }

            public int GetValues(object[] values)
            {
                //Array.Copy(this.Cells, values, Stuff.Min(values.Length, this.Cells.Length));
                throw new NotImplementedException();
            }

            public bool IsDBNull(int i)
            {
                var v = this[i];
                return v == DBNull.Value || v == null;
            }
        }
    }

    public class SimpleDataColumn : IDataColumn
    {
        public IDataTable Table { get; internal set; }
        public Type DataType { get; set; }
        public bool IsNullable { get; set; }
        public string ColumnName { get; set; }
        public int MaxLength { get; set; }
        public int Ordinal
        {
            get
            {
                if (Ordinal_p == -1 && Table != null)
                {
                    for (int z = 0; z < Table.Columns.Count; ++z)
                    {
                        if (Table.Columns[z] == this)
                        {
                            Ordinal_p = z;
                            break;
                        }
                    }
                }
                if (Ordinal_p == -1 || Table == null) return 0;
                return Ordinal_p;
            }
        }
        internal int Ordinal_p = -1;

        public SimpleDataColumn(string columnName = null)
        {
            ColumnName = columnName;
        }
    }

    public static class DataSetHelpers
    {
        public static DbDataReader CreateReader(this IDataTable dt)
        {
            throw new NotImplementedException();
        }

        public static bool Contains(this IEnumerable<IDataColumn> columns, string name)
        {
            foreach (var c in columns)
            {
                if (string.Compare(name, c.ColumnName, true) == 0) return true;
            }
            return false;
        }

        public static void Add(this ICollection<IDataColumn> columns, string columnName)
        {
            columns.Add(new SimpleDataColumn(columnName));
        }
    }
}
