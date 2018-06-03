using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "Joiner - Left",
        ComponentType = ComponentType.Transform,
        NoEditor = false,
        SupportsBackPressure = true,
        CurrentVersion = BasePipelineComponent.AssemblyComponentVersion,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class LeftJoinComponennt : BaseJoinerComponent
    {
        private static new class PropertyNames
        {
            public static class OutputProperties
            {
                public const int MatchlessId = BaseJoinerComponent.PropertyNames.OutputProperties.PrimaryOutputId;
            }
        }

        private class MyRuntimeData : JoinerRuntimeData
        {
            public int InputFingerprintsSampled;
            public int ProcessInputRootHits;
            public int ProcessInputRootMisses;
            public int ProcessInputRootFanoutHits;

            public MyRuntimeData(LeftJoinComponennt parent)
                : base(parent, true)
            { }
        }

        protected override RuntimeData ConstructRuntimeData()
            => new MyRuntimeData(this);

        private new MyRuntimeData RD
            => (MyRuntimeData)base.RD;

        public LeftJoinComponennt()
            : base(true)
        { }

        protected override void OnProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput)
        {
            ComponentMetaData.Name = "Joiner - Left";
            ComponentMetaData.Description = "Performs an left join of the 2 inputs.  Returns all rows from the left merged with matching rows on the right (null columns when missing). No fanout!";

            var output = ComponentMetaData.OutputCollection.New();
            output.ExclusionGroup = 1;
            output.SynchronousInputID = leftInput.ID;
            output.Name = "Left Join";
            output.Description = "Left rows with right columns when matched, and null right columns on a miss";
        }

        protected override DTSValidationStatus OnValidate(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IDTSOutputColumnCollection100 outputColumns, IList<string> commonFingerprints)
        {
            var ret = base.OnValidate(leftColumns, rightColumns, outputColumns, commonFingerprints);
            if (ret != DTSValidationStatus.VS_ISVALID) return ret;
            if (outputColumns.Count != rightColumns.Count - commonFingerprints.Count)//leftColumns.Count + rightColumns.Count - commonFingerprints.Count)
            {
                FireInformation(JoinerMessageCodes.ValidateError, $"Validate: output column count={outputColumns.Count}<>{rightColumns.Count - commonFingerprints.Count} but left={leftColumns.Count} right={rightColumns.Count} and common={commonFingerprints.Count}");
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            return DTSValidationStatus.VS_ISVALID;
        }

        protected override void DefineOutputs(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IList<string> commonFingerprints)
        {
            SetInputColumnUsage(DTSUsageType.UT_IGNORED);
            var pocs = SetPrimaryOutputColumnsToLeftInputColumns();
            for (int z = 0; z < rightColumns.Count; ++z)
            {
                var col = rightColumns[z];
                if (!commonFingerprints.Contains(col.CreateFingerprint()))
                {
                    var outCol = pocs.AddOutputColumn(col);
                    outCol.LineageID = col.LineageID;
                }
            }
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
                    if (RD.PrimaryOutputIsAttached)
                    {
                        var appends = AppendsByCommonFieldHash[fingerprint].SingleOrDefault();
                        if (appends != null)
                        {
                            ++RD.ProcessInputRootHits;
                            for (int z = 0; z < RD.OrderedAppendedPrimaryOutputColumnIndicees.Count; ++z)
                            {
                                var o = appends[z];
                                var index = RD.OrderedAppendedPrimaryOutputColumnIndicees[z];
                                buffer[index] = o;
                                //buffer.SetObject(col.DataType, z, o);
                            }
                        }
                        else
                        {
                            ++RD.ProcessInputRootMisses;
                            for (int z = 0; z < RD.OrderedAppendedColumnNames.Count; ++z)
                            {
                                var index = RD.OrderedAppendedPrimaryOutputColumnIndicees[z];
                                buffer.SetNull(index);
                                //buffer.SetObject(col.DataType, z, o);
                            }
                        }
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
