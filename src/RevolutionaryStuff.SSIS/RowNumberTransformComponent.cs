using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "Row Number Column",
        ComponentType = ComponentType.Transform,
        IconResource = "RevolutionaryStuff.SSIS.Resources.Icon1.ico")]
    public class RowNumberTransformComponent : BasePipelineComponent
    {
        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "Row Number Column";
            ComponentMetaData.Description = "Add a row number column to each row.";

            var p = ComponentMetaData.CustomPropertyCollection.New();
            p.Name = "OutputColumnName";
            p.Description = "Name of the new derived column";
            p.Value = "RowNumber";

            p = ComponentMetaData.CustomPropertyCollection.New();
            p.Name = "InitialValue";
            p.Description = "The initial value of the row number";
            p.Value = "1";

            p = ComponentMetaData.CustomPropertyCollection.New();
            p.Name = "Increment";
            p.Description = "The amount to increment";
            p.Value = "1";

            var left = ComponentMetaData.InputCollection.New();
            left.Name = "Input";
            var matched = ComponentMetaData.OutputCollection.New();
            matched.SynchronousInputID = left.ID;
            matched.Name = "Output";
            matched.Description = "The output with the new column.";
        }

        private string OutputColumnName => ComponentMetaData.CustomPropertyCollection["OutputColumnName"].Value as string;

        private int InitialValue
            => int.TryParse(ComponentMetaData.CustomPropertyCollection["InitialValue"].Value as string, out var i) ? i : 1;

        private int Incremement
            => int.TryParse(ComponentMetaData.CustomPropertyCollection["Increment"].Value as string, out var i) ? i : 1;

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
            if (string.IsNullOrEmpty(OutputColumnName)) return;
            DebuggerAttachmentWait();
            var input = ComponentMetaData.InputCollection[0].GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                input.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
            }
            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            outCols.RemoveAll();
            var outCol = outCols.New();
            outCol.Name = OutputColumnName;
            outCol.SetDataTypeProperties(DataType.DT_I4, 0, 0, 0, 0);
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
                        if (outCols.Count != 1)
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

        private ColumnBufferMapping InputRootBufferColumnIndicees;
        private ColumnBufferMapping OutputBufferColumnIndicees;

        public override void PreExecute()
        {
            base.PreExecute();
            InputRootBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.InputCollection[0]);
            OutputBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.OutputCollection[0], ComponentMetaData.InputCollection[0].Buffer);
        }

        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
            if (!ComponentMetaData.OutputCollection[0].IsAttached) return;

            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            int outputColumnPosition = OutputBufferColumnIndicees.PositionByColumnName[OutputColumnName];
            var inc = Incremement;
            while (buffer.NextRow())
            {
                buffer.SetObject(DataType.DT_I4, outputColumnPosition, RowNumber);
                RowNumber += inc;
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

        private int RowNumber;

        public override void PrepareForExecute()
        {
            base.PrepareForExecute();
            RowNumber = InitialValue;
        }
    }
}
