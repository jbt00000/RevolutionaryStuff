using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using RevolutionaryStuff.Core;
using System;
using System.Runtime.CompilerServices;

namespace RevolutionaryStuff.SSIS
{
    /// <remarks>https://www.simple-talk.com/sql/ssis/developing-a-custom-ssis-source-component/</remarks>
    /// <remarks>https://docs.microsoft.com/en-us/sql/integration-services/extending-packages-custom-objects-data-flow-types/developing-a-custom-transformation-component-with-synchronous-outputs</remarks>
    public abstract class BasePipelineComponent : PipelineComponent
    {
        protected static class CommonPropertyNames
        {
            public const string IgnoreCase = "IgnoreCase";
            public const string OutputColumnName = "OutputColumnName";
        }

        protected int GetCustomPropertyAsInt(string propertyName, int fallback = 0)
            => Parse.ParseInt32(GetCustomPropertyAsString(propertyName), fallback);

        protected bool GetCustomPropertyAsBool(string propertyName, bool fallback = false)
            => Parse.ParseBool(GetCustomPropertyAsString(propertyName), fallback);

        protected string GetCustomPropertyAsString(string propertyName, string fallback = null)
        {
            try
            {
                var p = ComponentMetaData.CustomPropertyCollection[propertyName];
                if (p != null)
                {
                    return (string)p.Value;
                }
            }
            catch (Exception)
            { }
            return fallback;
        }

        protected IDTSCustomProperty100 CreateCustomProperty(string name, string defaultValue, string description)
        {
            var p = ComponentMetaData.CustomPropertyCollection.New();
            p.Name = name;
            p.Description = description;
            p.Value = defaultValue;
            return p;
        }

        private enum BasePipelineInfoMessages
        {
            WaitingForDebuggerAttachment,
        }


        protected int StatusNotifyIncrement = 1000;

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
                        FireInformation(BasePipelineInfoMessages.WaitingForDebuggerAttachment, $"{this.GetType().Name} {z}/60");
                    }
#endif
                }
                DebuggerAttachmentWaitDone = true;
            }
        }

        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            DebuggerAttachmentWait();
//            base.ProcessInput(inputID, buffer);
            OnProcessInput(inputID, buffer);
        }

        protected abstract void OnProcessInput(int inputID, PipelineBuffer buffer);

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

        protected object GetObject(string colName, PipelineBuffer buffer, ColumnBufferMapping cbm)
            => GetObject(colName, cbm.ColumnByColumnName[colName].DataType, cbm.PositionByColumnName[colName], buffer, cbm);

        /// <remarks>https://technet.microsoft.com/en-us/library/ms345165(v=sql.110).aspx</remarks>
        protected object GetObject(string colName, DataType colDataType, int colIndex, PipelineBuffer buffer, ColumnBufferMapping cbm)
        {
            var n = cbm.PositionByColumnPosition[colIndex];
            if (buffer.IsNull(n)) return null;
            switch (colDataType)
            {
                case DataType.DT_BOOL:
                    return buffer.GetBoolean(n);
                case DataType.DT_I1:
                    return buffer.GetSByte(n);
                case DataType.DT_I2:
                    return buffer.GetInt16(n);
                case DataType.DT_I4:
                    return buffer.GetInt32(n);
                case DataType.DT_I8:
                    return buffer.GetInt64(n);
                case DataType.DT_UI1:
                    return buffer.GetByte(n);
                case DataType.DT_UI2:
                    return buffer.GetUInt16(n);
                case DataType.DT_UI4:
                    return buffer.GetUInt32(n);
                case DataType.DT_UI8:
                    return buffer.GetUInt64(n);
                case DataType.DT_R4:
                    return buffer.GetSingle(n);
                case DataType.DT_R8:
                    return buffer.GetDouble(n);
                case DataType.DT_DBDATE:
                    return buffer.GetDate(n);
                case DataType.DT_DATE:
                case DataType.DT_DBTIMESTAMP:
                case DataType.DT_DBTIMESTAMP2:
                case DataType.DT_FILETIME:
                    return buffer.GetDateTime(n);
                case DataType.DT_STR:
                case DataType.DT_WSTR:
                case DataType.DT_NTEXT:
                case DataType.DT_TEXT:
                    return buffer.GetString(n);
                case DataType.DT_NUMERIC:
                case DataType.DT_DECIMAL:
                case DataType.DT_CY:
                    return buffer.GetDecimal(n);
                case DataType.DT_GUID:
                    return buffer.GetGuid(n);
            }
            bool cancel = true;
            ComponentMetaData.FireError(123, "GetObject", string.Format("GetObject(colName={0}, colDataType={1}) is not yet supported", colName, colDataType), "", 0, out cancel);
            return null;
        }

        protected ColumnBufferMapping GetBufferColumnIndicees(IDTSInput100 input, int? overrideBuffer=null)
        {
            var bufferId = overrideBuffer.GetValueOrDefault(input.Buffer);
            var cbm = new ColumnBufferMapping();
            for (int x = 0; x < input.InputColumnCollection.Count; x++)
            {
                var column = input.InputColumnCollection[x];
                var offset = BufferManager.FindColumnByLineageID(bufferId, column.LineageID);
                cbm.Add(column, offset);
            }

            return cbm;
        }

        protected ColumnBufferMapping GetBufferColumnIndicees(IDTSOutput100 output, int? overrideBuffer = null)
        {
            //done via ternary operator instead of GetValueOrDefault so as to not dereference output.Buffer unless critical
            var bufferId = overrideBuffer.HasValue ? overrideBuffer.Value : output.Buffer;
            var cbm = new ColumnBufferMapping();
            for (int x = 0; x < output.OutputColumnCollection.Count; x++)
            {
                var column = output.OutputColumnCollection[x];
                var offset = BufferManager.FindColumnByLineageID(bufferId, column.LineageID);
                cbm.Add(column, offset);
            }
            return cbm;
        }
    }
}
