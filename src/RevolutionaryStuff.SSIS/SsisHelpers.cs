using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System;

namespace RevolutionaryStuff.SSIS
{
    public static class SsisHelpers
    {
        public static IDTSInputColumn100 FindByName(this IDTSInputColumnCollection100 cols, string name)
        {
            foreach (IDTSInputColumn100 c in cols)
            {
                if (0 == string.Compare(c.Name, name, true)) return c;
            }
            return null;
        }

        public static IDTSOutputColumn100 AddOutputColumn(this IDTSOutputColumnCollection100 output, IDTSInputColumn100 inCol, string newName = null)
        {
            var outCol = output.New();
            outCol.Name = newName ?? inCol.Name;
            outCol.SetDataTypeProperties(inCol.DataType, inCol.Length, inCol.Precision, inCol.Scale, inCol.CodePage);
            return outCol;
        }

        public static void AddOutputColumns(this IDTSOutputColumnCollection100 output, IDTSInputColumnCollection100 root)
        {
            for (int z = 0; z < root.Count; ++z)
            {
                var inCol = root[z];
                AddOutputColumn(output, inCol);
            }
        }

        public static void SetObject(this PipelineBuffer buffer, string colName, ColumnBufferMapping cbm, object val)
            => buffer.SetObject(cbm.GetColumnFromColumnName(colName).DataType, cbm.GetPositionFromColumnName(colName), val, colName);

        public static void SetObject(this PipelineBuffer buffer, DataType dt, int i, object val, string debugInfo=null)
        {
            try
            {
                if (val == null)
                {
                    buffer.SetNull(i);
                }
                else
                {
                    switch (dt)
                    {
                        case DataType.DT_GUID:
                            buffer.SetGuid(i, (Guid)val);
                            break;
                        case DataType.DT_BOOL:
                            buffer.SetBoolean(i, (bool)val);
                            break;
                        case DataType.DT_I1:
                            buffer.SetSByte(i, (sbyte)val);
                            break;
                        case DataType.DT_I2:
                            buffer.SetInt16(i, (System.Int16)val);
                            break;
                        case DataType.DT_I4:
                            buffer.SetInt32(i, (System.Int32)val);
                            break;
                        case DataType.DT_I8:
                            buffer.SetInt64(i, (System.Int64)val);
                            break;
                        case DataType.DT_UI1:
                            buffer.SetByte(i, (byte)val);
                            break;
                        case DataType.DT_UI2:
                            buffer.SetUInt16(i, (System.UInt16)val);
                            break;
                        case DataType.DT_UI4:
                            buffer.SetUInt32(i, (System.UInt32)val);
                            break;
                        case DataType.DT_UI8:
                            buffer.SetUInt64(i, (System.UInt64)val);
                            break;
                        case DataType.DT_R4:
                            buffer.SetSingle(i, (float)val);
                            break;
                        case DataType.DT_R8:
                            buffer.SetDouble(i, (double)val);
                            break;
                        case DataType.DT_STR:
                        case DataType.DT_WSTR:
                        case DataType.DT_NTEXT:
                        case DataType.DT_TEXT:
                            buffer.SetString(i, (string)val);
                            break;

                        case DataType.DT_DBDATE:
                            buffer.SetDate(i, (DateTime)val);
                            break;
                        case DataType.DT_DATE:
                        case DataType.DT_DBTIMESTAMP:
                        case DataType.DT_DBTIMESTAMP2:
                        case DataType.DT_FILETIME:
                            buffer.SetDateTime(i, (DateTime)val);
                            break;
                        case DataType.DT_NUMERIC:
                        case DataType.DT_DECIMAL:
                        case DataType.DT_CY:
                            buffer.SetDecimal(i, (decimal)val);
                            break;
                        default:
                            buffer.SetNull(i);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"debugInfo=[{debugInfo}] pos={i} dt=[{dt}] val=[{val}]", ex);
            }
        }
    }
}
