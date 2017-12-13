﻿using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "String Cleaning",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = false,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class StringCleaningComponent : BasePipelineComponent
    {
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
                input.SetUsageType(vcol.LineageID, IsStringDataType(vcol.DataType) ? DTSUsageType.UT_READWRITE : DTSUsageType.UT_IGNORED);
            }
        }

        private ColumnBufferMapping InputCbm;

        public override void PreExecute()
        {
            base.PreExecute();
            InputCbm = CreateColumnBufferMapping(ComponentMetaData.InputCollection[0]);
        }

        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);

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
                                var sIn = GetObject(col.Name, buffer, InputCbm) as string;
                                if (sIn != null)
                                {
                                    var sOut = sIn.ToString().Trim();
                                    sOut = sOut == "" ? null : sOut;
                                    var pos = InputCbm.PositionByColumnPosition[z];
                                    if (sOut == null)
                                    {
                                        buffer.SetNull(pos);
                                    }
                                    else
                                    {
                                        buffer.SetString(pos, sOut);
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
