using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using RevolutionaryStuff.Core;
using System.Collections.Generic;

namespace RevolutionaryStuff.SSIS
{
    /// <remarks>
    /// https://docs.microsoft.com/en-us/sql/integration-services/extending-packages-scripting-data-flow-script-component-examples/enhancing-an-error-output-with-the-script-component
    /// </remarks>
    [DtsPipelineComponent(
        DisplayName = "Error Appender",
        ComponentType = ComponentType.Transform,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class ErrorAppenderTransfromComponent : BasePipelineComponent
    {
        private static class PropertyNames
        {
            public static class Inputs
            {
                public const string ErrorCodeColumnName = "ErrorCode";
                public const string ErrorColumnColumnName = "ErrorColumn";
            }
            public static class Outputs
            {
                public const string ErrorColumnColumnName = "ErrorColumnName";
                public const string ErrorDescription = "ErrorDescription";
            }
        }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "Error Appender";
            ComponentMetaData.Description = "Appends detailed error information to the row";

            CreateCustomProperty(PropertyNames.Inputs.ErrorCodeColumnName, "ErrorCode", "Name of the DT_I4 column containing the error code");
            CreateCustomProperty(PropertyNames.Inputs.ErrorColumnColumnName, "ErrorColumn", "Name of the DT_I4 column containing the errored column number");

            var left = ComponentMetaData.InputCollection.New();
            left.Name = "In";
            var matched = ComponentMetaData.OutputCollection.New();
            matched.SynchronousInputID = left.ID;
            matched.Name = "Out";
        }

        public override IDTSCustomProperty100 SetComponentProperty(string propertyName, object propertyValue)
        {
            var ret = base.SetComponentProperty(propertyName, propertyValue);
            DefineOutputs();
            return ret;
        }

        public override void OnOutputPathAttached(int outputID)
        {
            base.OnOutputPathAttached(outputID);
            DefineOutputs();
        }

        private void DefineOutputs()
        {
            if (!ComponentMetaData.InputCollection[0].IsAttached) return;
            if (!ComponentMetaData.OutputCollection[0].IsAttached) return;
            DebuggerAttachmentWait();
            var input = ComponentMetaData.InputCollection[0].GetVirtualInput();
            var inErrorCode = GetCustomPropertyAsString(PropertyNames.Inputs.ErrorCodeColumnName).ToLower();
            var inErrorColumn = GetCustomPropertyAsString(PropertyNames.Inputs.ErrorColumnColumnName).ToLower();
            bool unicode = false;
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                var name = vcol.Name.ToLower();
                DTSUsageType usage;
                if (name == inErrorCode || name == inErrorColumn)
                {
                    usage = DTSUsageType.UT_READONLY;
                }
                else
                {
                    usage = DTSUsageType.UT_IGNORED;
                    if (vcol.DataType == DataType.DT_WSTR || vcol.DataType == DataType.DT_NTEXT)
                    {
                        unicode = true;
                    }
                }
                input.SetUsageType(vcol.LineageID, usage);
            }
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            outCols.RemoveAll();
            var outputType = unicode ? DataType.DT_WSTR : DataType.DT_STR;
            var outputCodePage = unicode ? 0 : BasePipelineComponent.EnUsCodePage;
            var outCol = outCols.New();
            outCol.Name = PropertyNames.Outputs.ErrorColumnColumnName;
            outCol.SetDataTypeProperties(outputType, 2000, 0, 0, outputCodePage);
            outCol = outCols.New();
            outCol.Name = PropertyNames.Outputs.ErrorDescription;
            outCol.SetDataTypeProperties(outputType, 2000, 0, 0, outputCodePage);
        }

        public override DTSValidationStatus Validate()
        {
            var ret = base.Validate();
            switch (ret)
            {
                case DTSValidationStatus.VS_ISVALID:
                    if (!ComponentMetaData.InputCollection[0].IsAttached)
                    {
                        ret = DTSValidationStatus.VS_ISBROKEN;
                    }
                    else
                    {
                        var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
                        if (outCols.Count != 2)
                        {
                            ret = DTSValidationStatus.VS_ISBROKEN;
                        }
                    }
                    break;
            }
            return ret;
        }

        public override void ReinitializeMetaData()
        {
            base.ReinitializeMetaData();
            DefineOutputs();
        }

        private readonly IDictionary<int, string> ErrorColumnNameByErrorColumnId = new Dictionary<int, string>();
        private readonly IDictionary<int, string> ErrorDescriptionByErrorCode = new Dictionary<int, string>();
        private ColumnBufferMapping InputRootBufferColumnIndicees;
        private ColumnBufferMapping OutputBufferColumnIndicees;

        public override void PreExecute()
        {
            base.PreExecute();
            InputRootBufferColumnIndicees = CreateColumnBufferMapping(ComponentMetaData.InputCollection[0]);
            OutputBufferColumnIndicees = CreateColumnBufferMapping(ComponentMetaData.OutputCollection[0], ComponentMetaData.InputCollection[0].Buffer);
        }

        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
            if (!ComponentMetaData.OutputCollection[0].IsAttached) return;

            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;

            var inErrorCode = GetCustomPropertyAsString(PropertyNames.Inputs.ErrorCodeColumnName).ToLower();
            var inErrorColumn = GetCustomPropertyAsString(PropertyNames.Inputs.ErrorColumnColumnName).ToLower();

            var outErrorDescPos = OutputBufferColumnIndicees.GetPositionFromColumnName(PropertyNames.Outputs.ErrorDescription);
            var outErrorColumnNamePos = OutputBufferColumnIndicees.GetPositionFromColumnName(PropertyNames.Outputs.ErrorColumnColumnName);

            var cmd = this.ComponentMetaData as IDTSComponentMetaData130;

            while (buffer.NextRow())
            {
                var errorCode = (int) GetObject(inErrorCode, buffer, InputRootBufferColumnIndicees);
                var errorColumnId = (int) GetObject(inErrorColumn, buffer, InputRootBufferColumnIndicees);
                var errorDesc = ErrorDescriptionByErrorCode.FindOrCreate(errorCode, () => cmd.GetErrorDescription(errorCode));
                var errorColumnName = ErrorColumnNameByErrorColumnId.FindOrCreate(errorColumnId, () => cmd.GetIdentificationStringByID(errorColumnId));

                buffer.SetObject(DataType.DT_STR, outErrorDescPos, errorDesc);
                buffer.SetObject(DataType.DT_STR, outErrorColumnNamePos, errorColumnName);
            }
        }

        PipelineBuffer OuputBuffer;

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            if (buffers.Length == 1)
            {
                OuputBuffer = buffers[0];
            }
        }
    }
}
