using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "String Cleaning",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = false,
        IconResource = "RevolutionaryStuff.SSIS.Resources.Icon1.ico")]
    public class StringCleaningComponent : BasePipelineComponent
    {
        public StringCleaningComponent()
            : base()
        { }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            ComponentMetaData.Name = "String Cleaning";
            ComponentMetaData.Description = "Cleans all string columns";
        }

        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);
            if (this.ComponentMetaData.InputCollection[0].IsAttached)
            {
                DefineOutputs();
            }
        }

        private void DefineOutputs()
        {
            var input = ComponentMetaData.InputCollection[0].GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                input.SetUsageType(vcol.LineageID, IsStringDataType(vcol.DataType) ? DTSUsageType.UT_READWRITE : DTSUsageType.UT_READONLY);
            }
        }

        private ColumnBufferMapping InputCbm;

        public override void PreExecute()
        {
            base.PreExecute();
            InputCbm = GetBufferColumnIndicees(ComponentMetaData.InputCollection[0]);
        }

        // this is the bit that actually does all the work.
        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);

            DebuggerAttachmentWait();

            base.ProcessInput(inputID, buffer);

            if (buffer != null)
            {
                if (!buffer.EndOfRowset)
                {
                    while (buffer.NextRow())
                    {
                        for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                        {
                            var col = input.InputColumnCollection[z];
                            if (IsStringDataType(col.DataType))
                            {
                                var sIn = GetObject(col.Name, col.DataType, z, buffer, InputCbm) as string;
                                if (sIn != null)
                                {
                                    var sOut = sIn.ToString();
                                    sOut = sOut == "" ? null : sOut;
                                    if (sIn != sOut)
                                    {
                                        buffer.SetString(InputCbm.ByColumnPosition[z], sOut);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
