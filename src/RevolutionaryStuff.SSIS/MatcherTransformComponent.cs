using System.Collections.Generic;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    /// <remarks>Ugh... Can't rename this class without breaking existing packages</remarks>
    [DtsPipelineComponent(
        DisplayName = "Joiner - Multitype",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = true,
        NoEditor = false,
        CurrentVersion = BasePipelineComponent.AssemblyComponentVersion,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class MatcherTransformComponent : BaseJoinerComponent
    {
        private static new class PropertyNames
        {
            public const string ThrowOnInnerJoinWithFanout = "ThrowOnInnerJoinWithFanout";

            public static class OutputProperties
            {
                public const int InnerJoinId = 0;
                public const int MatchlessId = 1;
                public const int LeftJoinId = 2;
            }
        }

        private class MyRuntimeData : JoinerRuntimeData
        {
            public int InputFingerprintsSampled;
            public int ProcessInputRootHits = 0;
            public int ProcessInputRootMisses = 0;
            public int ProcessInputRootFanoutHits = 0;

            public MyRuntimeData(MatcherTransformComponent parent)
                : base(parent, true)
            {
            }
        }

        protected override RuntimeData ConstructRuntimeData()
            => new MyRuntimeData(this);

        private new MyRuntimeData RD
            => (MyRuntimeData)base.RD;

        bool ThrowOnInnerJoinWithFanout => GetCustomPropertyAsBool(PropertyNames.ThrowOnInnerJoinWithFanout, true);

        IDTSOutput100 MatchesOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.InnerJoinId];
        IDTSOutputColumnCollection100 InnerJoinColumns => MatchesOutput.OutputColumnCollection;
        IDTSOutput100 OrphansOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.MatchlessId];
        IDTSOutputColumnCollection100 MatchlessColumns => OrphansOutput.OutputColumnCollection;
        IDTSOutput100 UnionsOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.LeftJoinId];
        IDTSOutputColumnCollection100 LeftJoinColumns => UnionsOutput.OutputColumnCollection;

        public MatcherTransformComponent()
            : base(false)
        { }

        protected override void OnProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput)
        {
            ComponentMetaData.Name = "Joiner - Multitype";
            ComponentMetaData.Description = "Performs a join of the 2 inputs (based on field name / simple data type) and the joined outputs (left/inner/left where right null).";

            var output = ComponentMetaData.OutputCollection.New();
            output.SynchronousInputID = 0;
            output.Name = "Inner Join";
            output.Description = "Inner Join semantics";

            output = ComponentMetaData.OutputCollection.New();
            output.SynchronousInputID = 0;
            output.Name = "Matchless Join";
            output.Description = "Left rows when there is no match in the right table";

            output = ComponentMetaData.OutputCollection.New();
            output.SynchronousInputID = 0;
            output.Name = "Left Join";
            output.Description = "Left Join semantics";

            CreateCustomProperty(PropertyNames.ThrowOnInnerJoinWithFanout, "1", "When {1,true} on the Inner Join output throw if there is fanout, when {0,false} follow normal inner join semantics.");
        }

        protected override void DefineOutputs(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IList<string> commonFingerprints)
        {
            SetInputColumnUsage(DTSUsageType.UT_READONLY); //to preserve original semantics of this component
            var matchedOutputColumns = InnerJoinColumns;
            matchedOutputColumns.RemoveAll();
            matchedOutputColumns.AddOutputColumns(leftColumns);
            var unionedOutputColumns = LeftJoinColumns;
            unionedOutputColumns.RemoveAll();
            unionedOutputColumns.AddOutputColumns(leftColumns);
            for (int z = 0; z < rightColumns.Count; ++z)
            {
                var col = rightColumns[z];
                if (!commonFingerprints.Contains(col.CreateFingerprint()))
                {
                    matchedOutputColumns.AddOutputColumn(col);
                    unionedOutputColumns.AddOutputColumn(col);
                }
            }
            var orphanOutputColumns = MatchlessColumns;
            orphanOutputColumns.RemoveAll();
            orphanOutputColumns.AddOutputColumns(leftColumns);
        }

        protected override DTSValidationStatus OnValidate(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IDTSOutputColumnCollection100 outputColumns, IList<string> commonFingerprints)
        {
            DTSValidationStatus ret = base.OnValidate(leftColumns, rightColumns, outputColumns, commonFingerprints);
            if (ret != DTSValidationStatus.VS_ISVALID) return ret;
            if (InnerJoinColumns.Count != leftColumns.Count + rightColumns.Count - commonFingerprints.Count)
            {
                ret = DTSValidationStatus.VS_ISCORRUPT;
            }
            else if (MatchlessColumns.Count != leftColumns.Count)
            {
                ret = DTSValidationStatus.VS_ISCORRUPT;
            }
            if (LeftJoinColumns.Count != leftColumns.Count + rightColumns.Count - commonFingerprints.Count)
            {
                ret = DTSValidationStatus.VS_ISCORRUPT;
            }
            return ret;
        }

        private ColumnBufferMapping InnerJoinCbm;
        private ColumnBufferMapping MatchlessCbm;
        private ColumnBufferMapping LeftJoinCbm;

        public override void PreExecute()
        {
            base.PreExecute();
            InnerJoinCbm = CreateColumnBufferMapping(MatchesOutput);
            MatchlessCbm = CreateColumnBufferMapping(OrphansOutput);
            LeftJoinCbm = CreateColumnBufferMapping(UnionsOutput);
            GetCommonInputFingerprints(true);
        }

        protected override void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            var isInnerJoinAttached = MatchesOutput.IsAttached;
            var isMatchlessAttached = OrphansOutput.IsAttached;
            var isLeftJoinAttached = UnionsOutput.IsAttached;
            var commonFingerprints = GetCommonInputFingerprints();
            var fingerprinter = new Fingerprinter(RD.IgnoreCase, RD.TrimThenNullifyEmptyStrings);
            var sourceVals = new List<object>();
            int rowsProcessed = 0;
            var throwOnInnerJoinWithFanout = ThrowOnInnerJoinWithFanout;
            while (buffer.NextRow())
            {
                fingerprinter.Clear();
                for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                {
                    var col = input.InputColumnCollection[z];
                    var colFingerprint = col.CreateFingerprint();
//                    var o = buffer[z];
                    var o = GetObject(col.Name, buffer, RD.LeftInputCbm);
                    if (commonFingerprints.Contains(colFingerprint))
                    {
                        fingerprinter.Include(col.Name, o);
                    }
                    sourceVals.Add(o);
                }
                string fingerprint = null;
                if (AppendsByCommonFieldHash.Count > 0)
                {
                    fingerprint = fingerprinter.FingerPrint;
                }
                if (fingerprint!=null && AppendsByCommonFieldHash.ContainsKey(fingerprint))
                {
                    ++RD.ProcessInputRootHits;
                    if (isInnerJoinAttached|| isLeftJoinAttached)
                    {
                        var appendRows = AppendsByCommonFieldHash[fingerprint];
                        if (throwOnInnerJoinWithFanout && isInnerJoinAttached && appendRows.Count > 1)
                        {
                            FireError(InformationMessageCodes.FanoutOnInnerJoinWhenProhibited, $"[{fingerprint}] yielded fanout of {appendRows.Count}");
                        }
                        foreach (var appends in appendRows)
                        {
                            ++RD.ProcessInputRootFanoutHits;
                            if (isInnerJoinAttached)
                            {
                                CopyToMatchedOutput(InnerJoinColumns, sourceVals, appends, InnerJoinBuffer, InnerJoinCbm);
                            }
                            if (isLeftJoinAttached)
                            {
                                CopyToMatchedOutput(LeftJoinColumns, sourceVals, appends, LeftJoinBuffer, LeftJoinCbm);
                            }
                        }
                    }
                }
                else
                {
                    ++RD.ProcessInputRootMisses;
                    if (isMatchlessAttached)
                    {
                        CopyToOrphannedOutput(MatchlessColumns, sourceVals, MatchlessBuffer, MatchlessCbm);
                    }
                    if (isLeftJoinAttached)
                    {
                        CopyToOrphannedOutput(LeftJoinColumns, sourceVals, LeftJoinBuffer, LeftJoinCbm);
                    }
                }
                sourceVals.Clear();
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
            FireInformation(InformationMessageCodes.MatchStats, $"hits={RD.ProcessInputRootHits}, fanoutHits={RD.ProcessInputRootFanoutHits}, misses={RD.ProcessInputRootMisses}, isFinal={buffer.EndOfRowset}");
        }

        protected override void OnProcessInputEndOfRowset(int inputID)
        {
            base.OnProcessInputEndOfRowset(inputID);
            InnerJoinBuffer.SetEndOfRowset();
            MatchlessBuffer.SetEndOfRowset();
            LeftJoinBuffer.SetEndOfRowset();
        }

        private static void CopyToMatchedOutput(IDTSOutputColumnCollection100 cc, IList<object> sources, IList<object> appends, PipelineBuffer buf, ColumnBufferMapping cbm)
        {
            buf.AddRow();
            CopyValsToBuffer(buf, cc, sources, 0, cbm);
            CopyValsToBuffer(buf, cc, appends, sources.Count, cbm);
        }

        private static void CopyToOrphannedOutput(IDTSOutputColumnCollection100 cc, IList<object> sources, PipelineBuffer buf, ColumnBufferMapping cbm)
        {
            buf.AddRow();
            CopyValsToBuffer(buf, cc, sources, 0, cbm);
        }

        private static void CopyValsToBuffer(PipelineBuffer buf, IDTSOutputColumnCollection100 cc, IList<object> vals, int offset, ColumnBufferMapping cbm)
        {
            for (int z = 0; z < vals.Count; ++z)
            {
                var col = cc[z + offset];
                var i = cbm.PositionByColumnPosition[z + offset];
                var o = vals[z];
                buf.SetObject(col.DataType, i, o);
            }
        }

        PipelineBuffer InnerJoinBuffer;
        PipelineBuffer MatchlessBuffer;
        PipelineBuffer LeftJoinBuffer;

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            if (buffers.Length == 3)
            {
                InnerJoinBuffer = buffers[PropertyNames.OutputProperties.InnerJoinId];
                MatchlessBuffer = buffers[PropertyNames.OutputProperties.MatchlessId];
                LeftJoinBuffer = buffers[PropertyNames.OutputProperties.LeftJoinId];
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
