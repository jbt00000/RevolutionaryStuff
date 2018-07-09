using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "Joiner - Inner",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = true,
        NoEditor = false,
        CurrentVersion = BasePipelineComponent.AssemblyComponentVersion,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class InnerJoinComponennt : BaseJoinerComponent
    {
        private static new class PropertyNames
        {
            public const string ErrorOnMisses = "ErrorOnMisses";

            public static class OutputProperties
            {
                public const int MatchlessId = 0;
            }
        }

        private class MyRuntimeData : JoinerRuntimeData
        {
            public readonly bool ErrorOnMisses;

            public MyRuntimeData(InnerJoinComponennt parent)
                : base(parent, true)
            {
                ErrorOnMisses = GetCustomPropertyAsBool(PropertyNames.ErrorOnMisses, true);
            }
        }

        protected override RuntimeData ConstructRuntimeData()
            => new MyRuntimeData(this);

        private new MyRuntimeData RD
            => (MyRuntimeData)base.RD;

        IDTSOutput100 InnerJoinOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.MatchlessId];
        IDTSOutputColumnCollection100 InnerJoinColumns => InnerJoinOutput.OutputColumnCollection;

        public InnerJoinComponennt()
            : base(true)
        { }

        protected override void OnProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput, IDTSOutput100 primaryOutput)
        {
            base.OnProvideComponentProperties(leftInput, rightInput, primaryOutput);
            ComponentMetaData.Name = "Joiner - Inner";
            ComponentMetaData.Description = "Performs an inner join of the 2 inputs.  Returns all rows from the left merged with matching rows on the right. No fanout!";

            primaryOutput.Name = "Inner Join";
            primaryOutput.Description = "Left rows when there is no match in the right table";

            CreateCustomProperty(PropertyNames.ErrorOnMisses, "1", "When {1,true} throw an error when there is no right match on a left row.");
        }

        protected override DTSValidationStatus OnValidate(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IDTSOutputColumnCollection100 outputColumns, IList<string> commonFingerprints)
        {
            var ret = base.OnValidate(leftColumns, rightColumns, outputColumns, commonFingerprints);
            if (ret != DTSValidationStatus.VS_ISVALID) return ret;
            if (outputColumns.Count != rightColumns.Count - commonFingerprints.Count)//leftColumns.Count + rightColumns.Count - commonFingerprints.Count)
            {
                FireInformation(MergingMessageCodes.ValidateError, $"Validate: output column count={outputColumns.Count}<>{rightColumns.Count - commonFingerprints.Count} but left={leftColumns.Count} right={rightColumns.Count} and common={commonFingerprints.Count}");
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
                    pocs.AddOutputColumn(col);
                }
            }
        }

        private int InputFingerprintsSampled;
        private int ProcessInputRootHits = 0;
        private int ProcessInputRootMisses = 0;
        private int ProcessInputRootFanoutHits = 0;

        protected override void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            var fingerprinter = RD.CreateFingerprinter();
            var sourceVals = new List<object>();
            int rowsProcessed = 0;
            while (buffer.NextRow())
            {
                fingerprinter.Clear();
                for (int z = 0; z < RD.OrderedCommonColumnNames.Count; ++z)
                {
                    var colName = RD.OrderedCommonColumnNames[z];
                    var o = buffer[RD.LeftInputCbm.GetPositionFromColumnName(colName)];
                    fingerprinter.Include(colName, o);
                    sourceVals.Add(o);
                }
                string fingerprint = null;
                if (AppendsByCommonFieldHash.Count > 0)
                {
                    fingerprint = fingerprinter.FingerPrint;
                    if (AppendsByCommonFieldHash.ContainsKey(fingerprint))
                    {
                        ++ProcessInputRootHits;
                        if (RD.PrimaryOutputIsAttached)
                        {
                            var appends = AppendsByCommonFieldHash[fingerprint].Single();
                            for (int z = 0; z < RD.OrderedAppendedColumnNames.Count; ++z)
                            {
                                var o = appends[z];
                                var index = RD.PrimaryOutputCbm.GetPositionFromColumnName(RD.OrderedAppendedColumnNames[z]);
                                buffer[index] = o;
                                //buffer.SetObject(col.DataType, z, o);
                            }
                            buffer.DirectRow(RD.PrimaryOutputId);
                        }
                    }
                    else if (RD.ErrorOnMisses)
                    {
                        FireError(InformationMessageCodes.Miss, $"No right match for {fingerprint}");
                    }
                    else
                    {
                        ++ProcessInputRootMisses;
                        if (RD.PrimaryOutputIsAttached)
                        {
                            //                            buffer.DirectRow(RD.PrimaryOutputId);
                        }
                    }
                }
                sourceVals.Clear();
                ++rowsProcessed;
                if (InputFingerprintsSampled < SampleSize)
                {
                    ++InputFingerprintsSampled;
                    FireInformation(InformationMessageCodes.ExampleFingerprint, fingerprinter.FingerPrint);
                }
                if (rowsProcessed % StatusNotifyIncrement == 0)
                {
                    FireInformation(InformationMessageCodes.MatchStats, $"hits={ProcessInputRootHits}, fanoutHits={ProcessInputRootFanoutHits}, misses={ProcessInputRootMisses}");
                }
            }
            FireInformation(InformationMessageCodes.RowsProcessed, $"{rowsProcessed}");
            FireInformation(InformationMessageCodes.MatchStats, $"hits={ProcessInputRootHits}, fanoutHits={ProcessInputRootFanoutHits}, misses={ProcessInputRootMisses}");
        }

        private enum InformationMessageCodes
        {
            RowsProcessed = 1,
            ExampleFingerprint = 2,
            AppendsByCommonFieldHash = 3,
            MatchStats = 4,
            Miss = 5,
            FanoutOnInnerJoinWhenProhibited = 8,

        }
    }
}
