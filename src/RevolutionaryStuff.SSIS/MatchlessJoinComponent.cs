using System.Collections.Generic;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "Joiner - Matchless",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = true,
        NoEditor = false,
        CurrentVersion = BasePipelineComponent.AssemblyComponentVersion,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class MatchlessJoinComponennt : BaseJoinerComponent
    {
        private class MyRuntimeData : JoinerRuntimeData
        {
            public int InputFingerprintsSampled;
            public int ProcessInputRootHits = 0;
            public int ProcessInputRootMisses = 0;
            public int ProcessInputRootFanoutHits = 0;

            public MyRuntimeData(MatchlessJoinComponennt parent)
                : base(parent, false)
            { }
        }

        protected override RuntimeData ConstructRuntimeData()
            => new MyRuntimeData(this);

        private new MyRuntimeData RD
            => (MyRuntimeData)base.RD;

        public MatchlessJoinComponennt()
            : base(true)
        { }

        protected override void OnProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput, IDTSOutput100 primaryOutput)
        {
            base.OnProvideComponentProperties(leftInput, rightInput, primaryOutput);
            ComponentMetaData.Name = "Joiner - Matchless";
            ComponentMetaData.Description = "Performs a join of the 2 inputs.  Returns all rows from the left for which there were no matches on the right.";

            primaryOutput.Name = "Matchless Join";
            primaryOutput.Description = "Left rows when there is no match in the right table";
        }

        protected override void DefineOutputs(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IList<string> commonFingerprints)
        {
            SetInputColumnUsage(DTSUsageType.UT_IGNORED);
            SetPrimaryOutputColumnsToLeftInputColumns();
        }

        protected override void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            var fingerprinter = RD.CreateFingerprinter();
            int rowsProcessed = 0;
            while (buffer.NextRow())
            {
                fingerprinter.Clear();
                for (int z = 0; z < RD.OrderedCommonColumnNames.Count; ++z)
                {
                    var colName = RD.OrderedCommonColumnNames[z];
                    var o = buffer[RD.LeftInputCbm.GetPositionFromColumnName(colName)];
                    fingerprinter.Include(colName, o);
                }
                string fingerprint = null;
                if (AppendsByCommonFieldHash.Count > 0)
                {
                    fingerprint = fingerprinter.FingerPrint;
                }
                if (fingerprint == null || !AppendsByCommonFieldHash.ContainsKey(fingerprint))
                {
                    ++RD.ProcessInputRootMisses;
                    if (RD.PrimaryOutputIsAttached)
                    {
                        buffer.DirectRow(RD.PrimaryOutputId);
                    }
                }
                ++rowsProcessed;
                if (RD.InputFingerprintsSampled < SampleSize)
                {
                    ++RD.InputFingerprintsSampled;
                    FireInformation(InformationMessageCodes.ExampleFingerprint, fingerprinter.FingerPrint);
                }
                if (rowsProcessed % StatusNotifyIncrement == 0)
                {
                    FireInformation(InformationMessageCodes.MatchStats, $"hits={RD.ProcessInputRootHits}, fanoutHits={RD.ProcessInputRootFanoutHits}, misses={RD.ProcessInputRootMisses}");
                }
            }
            FireInformation(InformationMessageCodes.RowsProcessed, $"{rowsProcessed}");
            FireInformation(InformationMessageCodes.MatchStats, $"hits={RD.ProcessInputRootHits}, fanoutHits={RD.ProcessInputRootFanoutHits}, misses={RD.ProcessInputRootMisses}");
        }

        private enum InformationMessageCodes
        {
            RowsProcessed = 1,
            ExampleFingerprint = 2,
            AppendsByCommonFieldHash = 3,
            MatchStats = 4,
            FanoutOnInnerJoinWhenProhibited = 8,
        }
    }
}
