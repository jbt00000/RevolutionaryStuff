using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using RevolutionaryStuff.Core.Collections;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.SSIS
{
    /// <remarks>Ugh... Can't rename this class without breaking existing packages</remarks>
    [DtsPipelineComponent(
        DisplayName = "The Joiner",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = true,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class MatcherTransformComponent : BasePipelineComponent
    {
        private static class PropertyNames
        {
            public const string IgnoreCase = CommonPropertyNames.IgnoreCase;

            public static class InputProperties
            {
                public const int LeftId = 0;
                public const string LeftName = "Left Input";
                public const int RightId = 1;
                public const string RightName = "Right Input";
            }

            public static class OutputProperties
            {
                public const int MatchesId = 0;
                public const int OrphansId = 1;
                public const int UnionsId = 2;
            }
        }

        IDTSInput100 LeftInput => ComponentMetaData.InputCollection[PropertyNames.InputProperties.LeftId];
        IDTSInputColumnCollection100 LeftColumns => LeftInput.InputColumnCollection;
        IDTSInput100 RightInput => ComponentMetaData.InputCollection[PropertyNames.InputProperties.RightId];
        IDTSInputColumnCollection100 RightColumns => RightInput.InputColumnCollection;
        IDTSOutput100 MatchesOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.MatchesId];
        IDTSOutputColumnCollection100 MatchesColumns => MatchesOutput.OutputColumnCollection;
        IDTSOutput100 OrphansOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.OrphansId];
        IDTSOutputColumnCollection100 OrphansColumns => OrphansOutput.OutputColumnCollection;
        IDTSOutput100 UnionsOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.UnionsId];
        IDTSOutputColumnCollection100 UnionsColumns => UnionsOutput.OutputColumnCollection;


        public MatcherTransformComponent()
            : base()
        { }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "The Joiner";
            ComponentMetaData.Description = "Performs a join of the 2 inputs (based on field name / simple data type) and the joined outputs (left/inner/left where right null).";

            var input = ComponentMetaData.InputCollection.New();
            input.Name = PropertyNames.InputProperties.LeftName;

            input = ComponentMetaData.InputCollection.New();
            input.Name = PropertyNames.InputProperties.RightName;

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

            CreateCustomProperty(PropertyNames.IgnoreCase, "1", "When {1,true} the match is case insensitive, when {0,false} it is case sensitive.");
        }

        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);
            if (LeftInput.IsAttached && RightInput.IsAttached)
            {
                DefineOutputs();
            }
        }

        private IList<string> GetComparisonColumnKeys(bool fireInformationMessages = false)
        {
            var leftCols = LeftColumns;
            var rightCols = RightColumns;
            var leftDefs = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            var rightDefs = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            for (int z = 0; z < leftCols.Count; ++z)
            {
                var col = leftCols[z];
                if (fireInformationMessages)
                {
                    FireInformation(InformationMessageCodes.LeftColumns, col.Name);
                }
                leftDefs.Add(CreateColumnFingerprint(col));
            }
            for (int z = 0; z < rightCols.Count; ++z)
            {
                var col = rightCols[z];
                if (fireInformationMessages)
                {
                    FireInformation(InformationMessageCodes.RightColumns, col.Name);
                }
                rightDefs.Add(CreateColumnFingerprint(rightCols[z]));
            }
            var commonDefs = new HashSet<string>(leftDefs.Intersect(rightDefs), Comparers.CaseInsensitiveStringComparer);
            if (fireInformationMessages)
            {
                foreach (var c in commonDefs)
                {
                    FireInformation(InformationMessageCodes.CommonColumns, c);
                }
            }
            return commonDefs.ToList();
        }

        private void DefineOutputs()
        {
            for (int z = 0; z < 2; ++z)
            {
                var input = ComponentMetaData.InputCollection[z].GetVirtualInput();
                foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
                {
                    input.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
                }
            }
            var leftCols = LeftColumns;
            var rightCols = RightColumns;
            var commonDefs = GetComparisonColumnKeys(true);
            var matchedOutputColumns = MatchesColumns;
            matchedOutputColumns.RemoveAll();
            matchedOutputColumns.AddOutputColumns(leftCols);
            var unionedOutputColumns = UnionsColumns;
            unionedOutputColumns.RemoveAll();
            unionedOutputColumns.AddOutputColumns(leftCols);
            for (int z = 0; z < rightCols.Count; ++z)
            {
                var col = rightCols[z];
                if (!commonDefs.Contains(CreateColumnFingerprint(col)))
                {
                    matchedOutputColumns.AddOutputColumn(col);
                    unionedOutputColumns.AddOutputColumn(col);
                }
            }
            var orphanOutputColumns = OrphansColumns;
            orphanOutputColumns.RemoveAll();
            orphanOutputColumns.AddOutputColumns(leftCols);
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
                    else
                    {
                        var leftCols = LeftColumns;
                        var rightCols = RightColumns;
                        var commonDefs = GetComparisonColumnKeys();
                        if (MatchesColumns.Count != leftCols.Count + rightCols.Count - commonDefs.Count)
                        {
                            ret = DTSValidationStatus.VS_NEEDSNEWMETADATA;
                        }
                        else if (OrphansColumns.Count != leftCols.Count)
                        {
                            ret = DTSValidationStatus.VS_NEEDSNEWMETADATA;
                        }
                        if (UnionsColumns.Count != leftCols.Count + rightCols.Count - commonDefs.Count)
                        {
                            ret = DTSValidationStatus.VS_NEEDSNEWMETADATA;
                        }
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

        private ColumnBufferMapping LeftInputCbm;
        private ColumnBufferMapping RightInputCbm;
        private ColumnBufferMapping MatchesCbm;
        private ColumnBufferMapping OrhansCbm;
        private ColumnBufferMapping UnionsCbm;

        public override void PreExecute()
        {
            base.PreExecute();
            if (GetCustomPropertyAsBool(PropertyNames.IgnoreCase, true))
            {
                AppendsByCommonFieldHash = new MultipleValueDictionary<string, object[]>(Comparers.CaseInsensitiveStringComparer);
            }
            else
            {
                AppendsByCommonFieldHash = new MultipleValueDictionary<string, object[]>();
            }
            LeftInputCbm = CreateColumnBufferMapping(LeftInput);
            RightInputCbm = CreateColumnBufferMapping(RightInput);
            MatchesCbm = CreateColumnBufferMapping(MatchesOutput);
            OrhansCbm = CreateColumnBufferMapping(OrphansOutput);
            UnionsCbm = CreateColumnBufferMapping(UnionsOutput);
            GetComparisonColumnKeys(true);
        }

        protected override void OnProcessInput(int inputId, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputId);
            switch (input.Name)
            {
                case PropertyNames.InputProperties.LeftName:
                    if (!LeftInputProcessed)
                    {
                        ProcessLeftInput(input, buffer);
                    }
                    break;
                case PropertyNames.InputProperties.RightName:
                    if (!RightInputProcessed)
                    {
                        ProcessRightInput(input, buffer);
                    }
                    break;
                default:
                    bool fireAgain = true;
                    ComponentMetaData.FireInformation(0, "", string.Format("Not expecting inputID={0}", inputId), "", 0, ref fireAgain);
                    throw new InvalidOperationException(string.Format("Not expecting inputID={0}", inputId));
            }
        }


        private int InputFingerprintsSampled;
        private int ProcessInputRootHits = 0;
        private int ProcessInputRootMisses = 0;
        private int ProcessInputRootFanoutHits = 0;

        private void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            var isMatchesAttached = MatchesOutput.IsAttached;
            var isOrphansAttached = OrphansOutput.IsAttached;
            var isUnionsAttached = UnionsOutput.IsAttached;
            var commonFingerprints = GetComparisonColumnKeys();
            var fingerprinter = new Fingerprinter();
            var sourceVals = new List<object>();
            int rowsProcessed = 0;
            while (buffer.NextRow())
            {
                fingerprinter.Clear();
                for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                {
                    var col = input.InputColumnCollection[z];
                    var colFingerprint = CreateColumnFingerprint(col);
                    var o = GetObject(col.Name, buffer, LeftInputCbm);
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
                    ++ProcessInputRootHits;
                    if (isMatchesAttached|| isUnionsAttached)
                    {
                        foreach (var appends in AppendsByCommonFieldHash[fingerprint])
                        {
                            ++ProcessInputRootFanoutHits;
                            if (isMatchesAttached)
                            {
                                CopyToMatchedOutput(MatchesColumns, sourceVals, appends, MatchesBuffer, MatchesCbm);
                            }
                            if (isUnionsAttached)
                            {
                                CopyToMatchedOutput(UnionsColumns, sourceVals, appends, UnionsBuffer, UnionsCbm);
                            }
                        }
                    }
                }
                else
                {
                    ++ProcessInputRootMisses;
                    if (isOrphansAttached)
                    {
                        CopyToOrphannedOutput(OrphansColumns, sourceVals, OrphansBuffer, OrhansCbm);
                    }
                    if (isUnionsAttached)
                    {
                        CopyToOrphannedOutput(UnionsColumns, sourceVals, UnionsBuffer, UnionsCbm);
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
                MatchesBuffer.SetEndOfRowset();
                OrphansBuffer.SetEndOfRowset();
                UnionsBuffer.SetEndOfRowset();
                LeftInputProcessed = true;
            }
        }

        public override void IsInputReady(int[] inputIDs, ref bool[] canProcess)
        {
            for (int i = 0; i < inputIDs.Length; i++)
            {
                int inputIndex = ComponentMetaData.InputCollection.GetObjectIndexByID(inputIDs[i]);
                bool can;
                switch (inputIndex)
                {
                    case PropertyNames.InputProperties.LeftId:
                        can = LeftInputProcessed || RightInputProcessed;
                        break;
                    case PropertyNames.InputProperties.RightId:
                        can = true; //!InputComparisonProcessed;
                        break;
                    default:
                        can = false;
                        break;
                }
                canProcess[i] = can;
            }
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

        PipelineBuffer MatchesBuffer;
        PipelineBuffer OrphansBuffer;
        PipelineBuffer UnionsBuffer;

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            if (buffers.Length == 3)
            {
                MatchesBuffer = buffers[PropertyNames.OutputProperties.MatchesId];
                OrphansBuffer = buffers[PropertyNames.OutputProperties.OrphansId];
                UnionsBuffer = buffers[PropertyNames.OutputProperties.UnionsId];
            }
        }

        public override void PrepareForExecute()
        {
            base.PrepareForExecute();
            LeftInputProcessed = false;
            RightInputProcessed = false;
        }

        private MultipleValueDictionary<string, object[]> AppendsByCommonFieldHash;
        private bool RightInputProcessed = false;
        private bool LeftInputProcessed = false;
        private int ComparisonFingerprintsSampled = 0;

        private void ProcessRightInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            int rowsProcessed = 0;
            var commonFingerprints = GetComparisonColumnKeys();
            var fingerprinter = new Fingerprinter();
            var appends = new List<object>();
            while (buffer.NextRow())
            {
                for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                {
                    var col = input.InputColumnCollection[z];
                    var colFingerprint = CreateColumnFingerprint(col);
                    var o = GetObject(col.Name, buffer, RightInputCbm);
                    if (commonFingerprints.Contains(colFingerprint))
                    {
                        fingerprinter.Include(col.Name, o);
                    }
                    else
                    {
                        appends.Add(o);
                    }
                }
                var fingerprint = fingerprinter.FingerPrint;
                AppendsByCommonFieldHash.Add(fingerprint, appends.ToArray());
                fingerprinter.Clear();
                appends.Clear();
                ++rowsProcessed;
                if (ComparisonFingerprintsSampled < SampleSize)
                {
                    ++ComparisonFingerprintsSampled;
                    FireInformation(InformationMessageCodes.ExampleFingerprint, fingerprint);
                }
            }
            FireInformation(InformationMessageCodes.RowsProcessed, $"{rowsProcessed}");
            FireInformation(InformationMessageCodes.AppendsByCommonFieldHash, $"{AppendsByCommonFieldHash.Count}/{AppendsByCommonFieldHash.AtomEnumerable.Count()}");
            RightInputProcessed = buffer.EndOfRowset;
        }

        private enum InformationMessageCodes
        {
            RowsProcessed = 1,
            ExampleFingerprint = 2,
            AppendsByCommonFieldHash = 3,
            MatchStats = 4,
            CommonColumns = 5,
            LeftColumns = 6,
            RightColumns = 7,
        }

        #region Helpers

        private static string CreateColumnFingerprint(IDTSInputColumn100 col)
        {
            string def;
            switch (col.DataType)
            {
                case DataType.DT_CY:
                case DataType.DT_DECIMAL:
                case DataType.DT_NUMERIC:
                case DataType.DT_R4:
                case DataType.DT_R8:
                    def = "decimal";
                    break;
                case DataType.DT_UI1:
                case DataType.DT_UI2:
                case DataType.DT_UI4:
                case DataType.DT_UI8:
                case DataType.DT_I1:
                case DataType.DT_I2:
                case DataType.DT_I4:
                case DataType.DT_I8:
                    def = "num";
                    break;
                case DataType.DT_STR:
                case DataType.DT_WSTR:
                case DataType.DT_NTEXT:
                case DataType.DT_TEXT:
                    def = "string";
                    break;
                default:
                    def = col.DataType.ToString();
                    break;
            }
            def = col.Name.Trim() + ";" + def;
            return def.ToLower();
        }
        #endregion
    }
}
