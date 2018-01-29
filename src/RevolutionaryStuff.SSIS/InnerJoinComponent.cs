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
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class InnerJoinComponennt : BaseJoinerComponent
    {
        private static class PropertyNames
        {
            public static class OutputProperties
            {
                public const int MatchlessId = 0;
            }
        }

        IDTSOutput100 InnerJoinOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.MatchlessId];
        IDTSOutputColumnCollection100 InnerJoinColumns => InnerJoinOutput.OutputColumnCollection;

        public InnerJoinComponennt()
            : base()
        { }

        protected override void ProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput)
        {
            ComponentMetaData.Name = "Joiner - Inner";
            ComponentMetaData.Description = "Performs an inner join of the 2 inputs.  Returns all rows from the left merged with matching rows on the right. No fanout!";

            var output = ComponentMetaData.OutputCollection.New();
            output.ExclusionGroup = 1;
            output.SynchronousInputID = leftInput.ID;
            output.Name = "Inner Join";
            output.Description = "Left rows when there is no match in the right table";
        }

        protected override void DefineOutputs()
        {
            SetInputColumnUsage(DTSUsageType.UT_IGNORED);
            var commonDefs = GetCommonInputFingerprints(true);
            var rightCols = RightColumns;
            for (int z = 0; z < rightCols.Count; ++z)
            {
                var col = rightCols[z];
                if (!commonDefs.Contains(col.CreateFingerprint()))
                {
                    InnerJoinOutput.OutputColumnCollection.AddOutputColumn(col);
                }
            }
        }

        public override DTSValidationStatus Validate()
        {
            var ret = base.Validate();
            switch (ret)
            {
                case DTSValidationStatus.VS_ISVALID:
                    if (!LeftInput.IsAttached || !RightInput.IsAttached)
                    {
                        ret = DTSValidationStatus.VS_ISBROKEN;
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


        private ColumnBufferMapping InnerJoinCbm;

        public override void PreExecute()
        {
            base.PreExecute();
            InnerJoinCbm = CreateColumnBufferMapping(InnerJoinOutput, ComponentMetaData.InputCollection[0].Buffer);
        }


        private int InputFingerprintsSampled;
        private int ProcessInputRootHits = 0;
        private int ProcessInputRootMisses = 0;
        private int ProcessInputRootFanoutHits = 0;

        protected override void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            var matchlessOutput = ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.MatchlessId];
            var isOutputAttached = InnerJoinOutput.IsAttached;
            var fingerprinter = new Fingerprinter(IgnoreCase, TrimThenNullifyEmptyStrings);
            var sourceVals = new List<object>();
            int rowsProcessed = 0;
            while (buffer.NextRow())
            {
                fingerprinter.Clear();
                for (int z = 0; z < OrderedCommonColumnNames.Count; ++z)
                {
                    var colName = OrderedCommonColumnNames[z];
                    var o = buffer[LeftInputCbm.GetPositionFromColumnName(colName)];
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
                        if (isOutputAttached)
                        {
                            var appends = AppendsByCommonFieldHash[fingerprint].Single();
                            for (int z = 0; z < OrderedAppendedColumnNames.Count; ++z)
                            {
                                var o = appends[z];
                                var index = InnerJoinCbm.GetPositionFromColumnName(OrderedAppendedColumnNames[z]);
                                buffer[index] = o;
                                //buffer.SetObject(col.DataType, z, o);
                            }
                            buffer.DirectRow(matchlessOutput.ID);
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
            if (buffer.EndOfRowset)
            {
                AllDone();
            }
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
