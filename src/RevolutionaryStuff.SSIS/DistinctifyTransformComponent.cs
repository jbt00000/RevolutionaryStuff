using System.Collections.Generic;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "Distinctify",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = false,
        IconResource = "RevolutionaryStuff.SSIS.Resources.Icon1.ico")]
    public class DistinctifyTransformComponent : BasePipelineComponent
    {
        public DistinctifyTransformComponent()
            : base()
        { }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "Distinctify";
            ComponentMetaData.Description = "A SSIS Data Flow Transformation Component that returns only the distinct rows.";

            var input = ComponentMetaData.InputCollection.New();
            input.Name = "Input";

            var output = ComponentMetaData.OutputCollection.New();
            output.ExclusionGroup = 1;
            output.Name = "Distinct Rows";
            output.SynchronousInputID = input.ID;

            output = ComponentMetaData.OutputCollection.New();
            output.ExclusionGroup = 1;
            output.Name = "Duplicate Rows";
            output.SynchronousInputID = input.ID;
        }

        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);
            var input = ComponentMetaData.InputCollection[0].GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                input.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
            }
        }


        private ColumnBufferMapping InputCbm;

        public override void PreExecute()
        {
            base.PreExecute();
            InputCbm = GetBufferColumnIndicees(ComponentMetaData.InputCollection[0]);
        }

        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
            var distinctOutput = ComponentMetaData.OutputCollection[0];
            var duplicateOutput = ComponentMetaData.OutputCollection[1];

            if (buffer != null)
            {
                if (!buffer.EndOfRowset)
                {
                    var data = new List<object>(input.InputColumnCollection.Count);
                    while (buffer.NextRow())
                    {
                        data.Clear();
                        ++InputRows;
                        for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                        {
                            var col = input.InputColumnCollection[z];
                            var o = GetObject(col.Name, buffer, InputCbm);
                            data.Add(o);
                        }
                        var key = Cache.CreateKey(data);
                        if (Fingerprints.Contains(key))
                        {
                            ++DuplicateRows;
                            buffer.DirectRow(duplicateOutput.ID);
                        }
                        else
                        {
                            Fingerprints.Add(key);
                            ++OutputRows;
                            buffer.DirectRow(distinctOutput.ID);
                        }
                        if (InputRows % StatusNotifyIncrement == 0)
                        {
                            FireInformation(InformationMessageCodes.MatchStats, $"InputRows={InputRows}, OutputRows={OutputRows}, DuplicateRows={DuplicateRows}, Fingerprints={Fingerprints.Count}");
                        }
                    }
                }
            }
        }

        private readonly HashSet<string> Fingerprints = new HashSet<string>();
        private int InputRows, OutputRows, DuplicateRows;


        private enum InformationMessageCodes
        {
            MatchStats = 1,
        }
    }
}
