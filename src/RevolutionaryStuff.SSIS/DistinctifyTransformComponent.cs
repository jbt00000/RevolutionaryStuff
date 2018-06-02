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
        NoEditor = false,
        CurrentVersion = BasePipelineComponent.AssemblyComponentVersion,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class DistinctifyTransformComponent : BasePipelineComponent
    {
        private static class PropertyNames
        {
            public const string IgnoreCase = "Ignore Case";
        }

        private class MyRuntimeData : RuntimeData
        {
            public readonly bool IgnoreCase;
            public readonly HashSet<string> Fingerprints = new HashSet<string>();
            public int InputRows, DistinctRowCount, DuplicateRowCount;

            public MyRuntimeData(DistinctifyTransformComponent parent)
                : base(parent)
            {
                IgnoreCase = GetCustomPropertyAsBool(PropertyNames.IgnoreCase);
            }
        }

        protected override RuntimeData ConstructRuntimeData()
            => new MyRuntimeData(this);

        private new MyRuntimeData RD 
            => (MyRuntimeData)base.RD;

        public DistinctifyTransformComponent()
            : base(true)
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

            /*
            output = ComponentMetaData.OutputCollection.New();
            output.ExclusionGroup = 1;
            output.Name = "Duplicate Rows";
            output.SynchronousInputID = input.ID;
            */

            CreateCustomProperty(PropertyNames.IgnoreCase, "0", "When 1, the comparisons are case insensitive; When 0, case is used");
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
            InputCbm = CreateColumnBufferMapping(ComponentMetaData.InputCollection[0]);
        }

        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
            var distinctOutput = ComponentMetaData.OutputCollection[0];
            //var duplicateOutput = ComponentMetaData.OutputCollection[1];

            if (buffer != null)
            {
                if (!buffer.EndOfRowset)
                {
                    var data = new List<object>(input.InputColumnCollection.Count);
                    while (buffer.NextRow())
                    {
                        data.Clear();
                        ++RD.InputRows;
                        for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                        {
                            var col = input.InputColumnCollection[z];
                            var o = GetObject(col.Name, buffer, InputCbm);
                            if (RD.IgnoreCase && o is string && o != null)
                            {
                                o = ((string)o).ToLower();
                            }
                            data.Add(o);
                        }
                        var key = Cache.CreateKey(data);
                        if (RD.Fingerprints.Contains(key))
                        {
                            ++RD.DuplicateRowCount;
                            //buffer.DirectRow(duplicateOutput.ID);
                        }
                        else
                        {
                            RD.Fingerprints.Add(key);
                            ++RD.DistinctRowCount;
                            buffer.DirectRow(distinctOutput.ID);
                        }
                        if (RD.InputRows % StatusNotifyIncrement == 0)
                        {
                            FireInformation(InformationMessageCodes.MatchStats, $"InputRows={RD.InputRows}, OutputRows={RD.DistinctRowCount}, DuplicateRows={RD.DuplicateRowCount}, Fingerprints={RD.Fingerprints.Count}");
                        }
                    }
                }
            }
        }

        private enum InformationMessageCodes
        {
            MatchStats = 1,
        }
    }
}
