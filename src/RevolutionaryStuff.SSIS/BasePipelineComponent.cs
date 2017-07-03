using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System;
using System.Runtime.CompilerServices;

namespace RevolutionaryStuff.SSIS
{
    /// <remarks>https://www.simple-talk.com/sql/ssis/developing-a-custom-ssis-source-component/</remarks>
    /// <remarks>https://docs.microsoft.com/en-us/sql/integration-services/extending-packages-custom-objects-data-flow-types/developing-a-custom-transformation-component-with-synchronous-outputs</remarks>
    public abstract class BasePipelineComponent : PipelineComponent
    {
        private enum BasePipelineInfoMessages
        {
            WaitingForDebuggerAttachment,
        }

        private bool DebuggerAttachmentWaitDone;

        protected void DebuggerAttachmentWait()
        {
            lock (this)
            {
                if (!DebuggerAttachmentWaitDone)
                {
#if false
                    for (int z = 0; z < 60; ++z)
                    {
                        System.Threading.Thread.Sleep(1000);
                        FireInformation(BasePipelineInfoMessages.WaitingForDebuggerAttachment, $"{z}/60");
                    }
#endif
                }
                DebuggerAttachmentWaitDone = true;
            }
        }

        protected BasePipelineComponent()
        {
        }

        public override IDTSOutputColumn100 InsertOutputColumnAt(
                                         int outputID,
                                         int outputColumnIndex,
                                         string name,
                                         string description)
        {
            throw new Exception(string.Format("Fail to add output column name to {0} ", ComponentMetaData.Name), null);
        }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();

            ComponentMetaData.ContactInfo = "jason@jasonthomas.com";
        }

        protected void FireInformation<TMessageCode>(TMessageCode code, string message, [CallerMemberName] string caller = null) where TMessageCode : struct
        {
            bool fireAgain = true;
            ComponentMetaData.FireInformation((int)(object)code, $"{ComponentMetaData.Name}.{caller}", $"{code}: {message}", "", 0, ref fireAgain);
        }

        protected static bool IsStringDataType(DataType dt)
        {
            switch (dt)
            {
                case DataType.DT_STR:
                case DataType.DT_WSTR:
                case DataType.DT_NTEXT:
                case DataType.DT_TEXT:
                    return true;
                default:
                    return false;
            }
        }

        protected object GetObject(string colName, DataType colDataType, int colIndex, PipelineBuffer buffer, ColumnBufferMapping cbm)
        {
            var n = colIndex;
            if (buffer.IsNull(n)) return null;
            switch (colDataType)
            {
                case DataType.DT_BOOL:
                    return buffer.GetBoolean(n);
                case DataType.DT_I4:
                    return buffer.GetInt32(n);
                case DataType.DT_I2:
                    return buffer.GetInt16(n);
                case DataType.DT_I8:
                    return buffer.GetInt64(n);
                case DataType.DT_DATE:
                    return buffer.GetDate(n);
                case DataType.DT_STR:
                case DataType.DT_WSTR:
                case DataType.DT_NTEXT:
                case DataType.DT_TEXT:
                    return buffer.GetString(n);
            }
            bool cancel = true;
            ComponentMetaData.FireError(123, "GetObject", string.Format("GetObject(colName={0}, colDataType={1}) is not yet supported", colName, colDataType), "", 0, out cancel);
            return null;
        }

        protected ColumnBufferMapping GetBufferColumnIndicees(IDTSInput100 input)
        {
            var cbm = new ColumnBufferMapping();
            for (int x = 0; x < input.InputColumnCollection.Count; x++)
            {
                var column = input.InputColumnCollection[x];
                var offset = BufferManager.FindColumnByLineageID(input.Buffer, column.LineageID);
                cbm.Add(column.Name, offset);
            }

            return cbm;
        }

        protected ColumnBufferMapping GetBufferColumnIndicees(IDTSOutput100 output)
        {
            var cbm = new ColumnBufferMapping();
            for (int x = 0; x < output.OutputColumnCollection.Count; x++)
            {
                var column = output.OutputColumnCollection[x];
                var offset = BufferManager.FindColumnByLineageID(output.Buffer, column.LineageID);
                cbm.Add(column.Name, offset);
            }
            return cbm;
        }

        protected static void CopyColumnDefinition(IDTSOutputColumnCollection100 output, IDTSInputColumn100 inCol)
        {
            var outCol = output.New();
            outCol.Name = inCol.Name;
            outCol.SetDataTypeProperties(inCol.DataType, inCol.Length, inCol.Precision, inCol.Scale, inCol.CodePage);
        }

        protected static void AddOutputColumns(IDTSInputColumnCollection100 root, IDTSOutputColumnCollection100 output)
        {
            for (int z = 0; z < root.Count; ++z)
            {
                var inCol = root[z];
                CopyColumnDefinition(output, inCol);
            }
        }
    }
}
