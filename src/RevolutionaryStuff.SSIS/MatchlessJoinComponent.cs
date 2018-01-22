using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    /// <remarks>Ugh... Can't rename this class without breaking existing packages</remarks>
    [DtsPipelineComponent(
        DisplayName = "Joiner - Matchless",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = true,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class MatchlessJoinComponennt : BaseJoinerComponent
    {
        private static class PropertyNames
        {
            public static class OutputProperties
            {
                public const int MatchlessId = 0;
            }
        }

        IDTSOutput100 OrphansOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.MatchlessId];
        IDTSOutputColumnCollection100 MatchlessColumns => OrphansOutput.OutputColumnCollection;

        public MatchlessJoinComponennt()
            : base()
        { }

        protected override void ProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput)
        {
            ComponentMetaData.Name = "Joiner - Matchless";
            ComponentMetaData.Description = "Performs a join of the 2 inputs.  Returns all rows from the left for which there were no matches on the right.";

            var output = ComponentMetaData.OutputCollection.New();
            output.ExclusionGroup = 1;
            output.SynchronousInputID = leftInput.ID;
            output.Name = "Matchless Join";
            output.Description = "Left rows when there is no match in the right table";
        }

        protected override void DefineOutputs()
        {
            SetInputColumnUsage(DTSUsageType.UT_IGNORED); 
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

        private int InputFingerprintsSampled;
        private int ProcessInputRootHits = 0;
        private int ProcessInputRootMisses = 0;
        private int ProcessInputRootFanoutHits = 0;

        protected override void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            var matchlessOutput = ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.MatchlessId];
            var isMatchlessAttached = OrphansOutput.IsAttached;
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
                }
                if (fingerprint == null || !AppendsByCommonFieldHash.ContainsKey(fingerprint))
                {
                    ++ProcessInputRootMisses;
                    if (isMatchlessAttached)
                    {
                        buffer.DirectRow(matchlessOutput.ID);
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
