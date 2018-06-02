using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "String Cleaning",
        ComponentType = ComponentType.Transform,
        NoEditor = false,
        CurrentVersion = BasePipelineComponent.AssemblyComponentVersion,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class StringCleaningComponent : BasePipelineComponent
    {
        public StringCleaningComponent()
            : base(true)
        { }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            ComponentMetaData.Name = "String Cleaning";
            ComponentMetaData.Description = "Trims all string columns and replaces with null when empty.";
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
                input.SetUsageType(vcol.LineageID, vcol.DataType.IsStringDataType() ? DTSUsageType.UT_READWRITE : DTSUsageType.UT_IGNORED);
            }
        }

        int RowsProcessed = 0;
        int CellsProcessed = 0;
        int CellsUpdated = 0;

        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var cbm = RD.InputColumnBufferMappings[0];
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
            var colCount = input.InputColumnCollection.Count;
            while (buffer.NextRow())
            {
                for (int z = 0; z < colCount; ++z)
                {
                    var pos = cbm.PositionByColumnPosition[z];
                    ++CellsProcessed;
                    if (buffer.IsNull(pos)) continue;
                    var sIn = buffer[pos] as string;
                    if (sIn == null) continue;
                    if (sIn.Length == 0)
                    {
                        buffer.SetNull(pos);
                        ++CellsUpdated;
                    }
                    else
                    {
                        var sOut = sIn.Trim();
                        if (sOut.Length == 0)
                        {
                            buffer.SetNull(pos);
                            ++CellsUpdated;
                        }
                        else if (sIn != sOut)
                        {
                            buffer.SetString(pos, sOut);
                            ++CellsUpdated;
                        }
                    }
                }
                ++RowsProcessed;
            }
        }

        protected override void OnProcessInputEndOfRowset(int inputID)
        {
            FireInformation(4324, $"rowsProcessed={RowsProcessed} cellsProcessed={CellsProcessed} cellsUpdated={CellsUpdated}");
            base.OnProcessInputEndOfRowset(inputID);
        }
    }
}
