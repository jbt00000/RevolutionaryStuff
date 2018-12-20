using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace RevolutionaryStuff.ETL
{
    /// <remarks>
    /// https://www.clicketyclick.dk/databases/xbase/format/dbf.html#DBF_NOTE_15_SOURCE
    /// http://www.dbf2002.com/dbf-file-format.html
    /// http://dbase.com/Knowledgebase/INT/db7_file_fmt.htm
    /// https://msdn.microsoft.com/en-us/library/aa975386(v=vs.71).aspx
    /// https://msdn.microsoft.com/en-us/library/aa975374(v=vs.71).aspx
    /// </remarks>
    public static class DBaseHelpers
    {
        [Flags]
        private enum TableFlags : byte
        {
            StructuralCdx = 1,
            Memo = 2,
            Database = 4,
        }

        private enum FieldTypes
        {
            Character,
            Currency,
            Numeric,
            Float,
            Date,
            DateTime,
            Double,
            Integer,
            Logical,
            Memo,
            General,
            CharacterBinary,
            MemoBinary,
            Picture,
            Autoincrement,
            DoubleL7,
            Timestamp,
        }

        [Flags]
        private enum FieldFlags : byte
        {
            SystemColumn = 1,
            CanStoreNullValues = 2,
            BinaryColumn = 4,
            AutoIncrementing = 0xC,
        }

        private class DbtBlockRef
        {
            public readonly int DbtBlockNumber;

            public DbtBlockRef(int i)
            {
                DbtBlockNumber = i;
            }
        }

        private class FieldStructure
        {
            public readonly string Fieldname;
            public readonly FieldTypes FieldType;
            public readonly FieldFlags FieldFlags;
            public readonly int FieldLength;
            public readonly int NumberOfDecimalPlaces;
            public readonly int AutoIncrementNext;
            public readonly int AutoIncrementStep;

            public override string ToString()
                => $"{this.GetType().Name} name=\"{Fieldname}\" type={FieldType} len={FieldLength}";

            public object Parse(byte[] buf, int offset)
            {
                string s;
                char ch;
                int n;
                switch (FieldType)
                {
                    case FieldTypes.Character:
                        s = Raw.Buf2String(buf, offset, this.FieldLength);
                        s = StringHelpers.TrimOrNull(s);
                        return s;
                    case FieldTypes.Integer:
                    case FieldTypes.Autoincrement:
                        return Raw.ReadInt32FromBuf(buf, offset);
                    case FieldTypes.Logical:
                        ch = (char)buf[offset];
                        if (ch == 'T') return true;
                        if (ch == 'F') return false;
                        return DBNull.Value;
                    case FieldTypes.Numeric:
                        s = Raw.Buf2String(buf, offset, this.FieldLength).Trim();
                        if (s == "") return DBNull.Value;
                        return double.Parse(s);
                    case FieldTypes.Date:
                        s = Raw.Buf2String(buf, offset, this.FieldLength).Trim();
                        if (s == "") return DBNull.Value;
                        var dt = new DateTime(
                            int.Parse(s.Substring(0, 4)),
                            int.Parse(s.Substring(4, 2)),
                            int.Parse(s.Substring(6, 2)));
                        if (dt.Year < 1900) return DBNull.Value;
                        if (dt.Year > 9999) return DBNull.Value;
                        return dt;
                    case FieldTypes.Memo:
                    case FieldTypes.MemoBinary:
                        n = Raw.ReadInt32FromBuf(buf, offset);
                        if (n == 0) return DBNull.Value;
                        s = Raw.Buf2String(buf, offset, FieldLength);
                        return new DbtBlockRef(n);
                    //                        return new DbtBlockRef(int.Parse(s));
                    case FieldTypes.Currency:
                    case FieldTypes.Float:
                    case FieldTypes.DateTime:
                    case FieldTypes.Double:
                    case FieldTypes.DoubleL7:
                    case FieldTypes.Timestamp:
                    case FieldTypes.General:
                    case FieldTypes.CharacterBinary:
                    case FieldTypes.Picture:
                        throw new NotImplementedException();
                    default:
                        throw new UnexpectedSwitchValueException(FieldType);
                }
            }

            public DataColumn ToDataColumn()
            {
                var dc = new DataColumn(Fieldname);
                switch (FieldType)
                {
                    case FieldTypes.Character:
                        dc.DataType = typeof(string);
                        dc.MaxLength = FieldLength;
                        dc.Unicode(false);
                        break;
                    case FieldTypes.Currency:
                        dc.DataType = typeof(double);
                        break;
                    case FieldTypes.Numeric:
                        dc.DataType = typeof(double);
                        break;
                    case FieldTypes.Float:
                        dc.DataType = typeof(double);
                        break;
                    case FieldTypes.Date:
                    case FieldTypes.DateTime:
                        dc.DataType = typeof(DateTime);
                        break;
                    case FieldTypes.Double:
                        dc.DataType = typeof(double);
                        break;
                    case FieldTypes.Integer:
                        dc.DataType = typeof(int);
                        break;
                    case FieldTypes.Autoincrement:
                        dc.DataType = typeof(int);
                        break;
                    case FieldTypes.DoubleL7:
                        dc.DataType = typeof(double);
                        break;
                    case FieldTypes.Timestamp:
                        dc.DataType = typeof(DateTime);
                        break;
                    case FieldTypes.Logical:
                        dc.DataType = typeof(bool);
                        break;
                    case FieldTypes.Memo:
                        dc.DataType = typeof(string);
                        dc.MaxLength = FieldLength;
                        break;
                    case FieldTypes.General:
                    case FieldTypes.CharacterBinary:
                    case FieldTypes.MemoBinary:
                    case FieldTypes.Picture:
                        throw new NotImplementedException();
                    default:
                        throw new UnexpectedSwitchValueException(FieldType);
                }
                dc.AllowDBNull = FieldFlags.HasFlag(FieldFlags.CanStoreNullValues);
                return dc;
            }

            public FieldStructure(byte[] buf)
            {
                Requires.ArrayArg(buf, 0, 32, nameof(buf));
                Fieldname = "";
                for (int z = 0; z < 10; ++z)
                {
                    var ch = (char)buf[z];
                    if (ch == 0) break;
                    Fieldname += ch;
                }
                switch ((char)buf[11])
                {
                    case 'C':
                        FieldType = FieldTypes.Character;
                        break;
                    case 'Y':
                        FieldType = FieldTypes.Currency;
                        break;
                    case 'N':
                        FieldType = FieldTypes.Numeric;
                        break;
                    case 'F':
                        FieldType = FieldTypes.Float;
                        break;
                    case 'D':
                        FieldType = FieldTypes.Date;
                        break;
                    case 'T':
                        FieldType = FieldTypes.DateTime;
                        break;
                    case 'B':
                        FieldType = FieldTypes.Double;
                        break;
                    case 'I':
                        FieldType = FieldTypes.Integer;
                        break;
                    case 'L':
                        FieldType = FieldTypes.Logical;
                        break;
                    case 'M':
                        FieldType = FieldTypes.Memo;
                        break;
                    case 'G':
                        FieldType = FieldTypes.General;
                        break;
                    //case 'C':
                    //case 'M':
                    case 'P':
                        FieldType = FieldTypes.Picture;
                        break;
                    case '+':
                        FieldType = FieldTypes.Autoincrement;
                        break;
                    case 'O':
                        FieldType = FieldTypes.DoubleL7;
                        break;
                    case '@':
                        FieldType = FieldTypes.Timestamp;
                        break;
                    default:
                        throw new UnexpectedSwitchValueException(buf[11]);
                }
                FieldLength = buf[16];
                NumberOfDecimalPlaces = buf[17];
                FieldFlags = (FieldFlags)buf[18];
                AutoIncrementNext = Raw.ReadInt32FromBuf(buf, 19);
                AutoIncrementStep = Raw.ReadInt32FromBuf(buf, 23);
            }
        }

        private static object GetMemoBlockData(Stream memoStream, int blockNum, int blockSize)
        {
            memoStream.Position = blockSize * blockNum;
            var header = new byte[8];
            memoStream.ReadExactSize(header);
            var type = Raw.ReadInt32BeFromBuf(header, 0);
            var len = Raw.ReadInt32BeFromBuf(header, 4);
            var dataBuf = new byte[len];
            memoStream.ReadExactSize(dataBuf);
            switch (type)
            {
                case 1: //string
                    try
                    {
                        var s = Raw.Buf2String(dataBuf);
                        s = StringHelpers.TrimOrNull(s);
                        return s;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                        return null;
                    }
                case 0: //picture
                    return dataBuf;
                default:
                    throw new UnexpectedSwitchValueException(type);
            }
        }

        public static DataTable Load(Stream st, Stream memoStream = null)
        {
            Requires.ReadableStreamArg(st, nameof(st));

            var header = new byte[32];
            st.ReadExactSize(header);
            if (header[31] != 0)
            {
                throw new FormatException("Header character 31 must be a 0");
            }
            var signature = new System.Collections.BitArray(new byte[1] { header[0] });
            var hasDBT = signature[7];
            var numRecords = Raw.ReadInt32FromBuf(header, 4);
            var firstOffset = Raw.ReadInt16FromBuf(header, 8);
            var recordSize = Raw.ReadInt16FromBuf(header, 10);
            var flag = (TableFlags)header[28];
            var dt = new DataTable();
            var fields = new List<FieldStructure>();
            var fieldBuf = new byte[32];
            for (;;)
            {
                st.ReadExactSize(fieldBuf);
                if (fieldBuf[0] == 0x0d) break;
                var field = new FieldStructure(fieldBuf);
                fields.Add(field);
                dt.Columns.Add(field.ToDataColumn());
            }
            st.Position -= (32 -1);
            if (fieldBuf[1] == 0)
            {
                st.Position += 1;
            }
            if (hasDBT)
            {
                st.Position += 262;
            }
            var rowBuf = new byte[recordSize];
            var rawRows = new List<object[]>(numRecords);
            var memos = new List<Tuple<DbtBlockRef, FieldStructure, int, int>>();
            if (memoStream == null) memos = null;
            for (int absRowNum = 0; absRowNum < numRecords; ++absRowNum)
            {
                st.ReadExactSize(rowBuf);
                if ((char)rowBuf[0] == '*') continue;
                if ((char)rowBuf[0] != ' ')
                {
                    throw new FormatException("First character in row of .dbf file must be a space or a *");
                }
                var rowVals = new object[fields.Count];
                int offset = 1;
                for (int col = 0; col < fields.Count; ++col)
                {
                    DataColumn dc = dt.Columns[col];
                    var field = fields[col];
                    var v = field.Parse(rowBuf, offset);
                    bool allowNulls = false;
                    if (v is DbtBlockRef)
                    {
                        rowVals[col] = DBNull.Value;
                        if (memos != null)
                        {
                            memos.Add(Tuple.Create(v as DbtBlockRef, field, rawRows.Count, col));
                        }
                        allowNulls = true;
                    }
                    else
                    {
                        rowVals[col] = v;
                        allowNulls = v == null || v == DBNull.Value;
                    }
                    if (allowNulls && !dc.AllowDBNull)
                    {
                        dc.AllowDBNull = true;
                    }
                    offset += field.FieldLength;
                }
                if (memos == null)
                {
                    dt.Rows.Add(rowVals);
                }
                else
                {
                    rawRows.Add(rowVals);
                }
            }
            if (memos != null)
            {
                var fptHeaderBuf = new byte[512];
                memoStream.ReadExactSize(fptHeaderBuf, 0, fptHeaderBuf.Length);
                var blockSize = Raw.ReadInt16BeFromBuf(fptHeaderBuf, 6);
                foreach (var m in memos)
                {
                    var blockNum = m.Item1.DbtBlockNumber;
                    rawRows[m.Item3][m.Item4] = DBNull.Value;
                    var sdata = GetMemoBlockData(memoStream, blockNum, blockSize) as string;
                    if (sdata != null)
                    {
                        var col = dt.Columns[m.Item4];
                        col.MaxLength = Math.Max(col.MaxLength, sdata.Length);
                        rawRows[m.Item3][m.Item4] = sdata;
                    }
                }
                foreach (var rv in rawRows)
                {
                    dt.Rows.Add(rv);
                }
            }
            return dt;
        }
    }
}
