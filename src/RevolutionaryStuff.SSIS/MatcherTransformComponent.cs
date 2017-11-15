﻿using System;
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
                public const string Root = "Left Input";
                public const int RootId = 0;
                public const string Comparison = "Right Input";
                public const int ComparisonId = 1;
            }

            public static class OutputProperties
            {
                public const string Matches = "Match Output";
                public const string Orphans = "No Matched Output";
            }
        }

        public MatcherTransformComponent()
            : base()
        { }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "The Joiner";
            ComponentMetaData.Description = "A SSIS Data Flow Transformation Component that auto joins 2 inputs (based on field name / data type) and returns the matched (union of all columns, same rows as an inner join) and orphanned (* from the left table and unmatched rows from left table) tables.";

            var left = ComponentMetaData.InputCollection.New();
            left.Name = PropertyNames.InputProperties.Root;
            var right = ComponentMetaData.InputCollection.New();
            right.Name = PropertyNames.InputProperties.Comparison;
            var matched = ComponentMetaData.OutputCollection.New();
            matched.SynchronousInputID = 0;
            matched.Name = PropertyNames.OutputProperties.Matches;
            matched.Description = "Root rows that have have corresponding matches in the Comparison";
            var notMatched = ComponentMetaData.OutputCollection.New();
            notMatched.SynchronousInputID = 0;
            notMatched.Name = PropertyNames.OutputProperties.Orphans;
            notMatched.Description = "Root rows that have no corresponding matches in the Comparison";

            CreateCustomProperty(PropertyNames.IgnoreCase, "1", "When {1,true} the match is case insensitive, when {0,false} it is case sensitive.");
        }

        public override IDTSCustomProperty100 SetComponentProperty(string propertyName, object propertyValue)
        {
            switch (propertyName)
            {
                case PropertyNames.OutputProperties.Matches:
                    ComponentMetaData.OutputCollection[0].OutputColumnCollection.AddOutputColumns(ComponentMetaData.InputCollection[0].InputColumnCollection);
                    break;
                case PropertyNames.OutputProperties.Orphans:
                    ComponentMetaData.OutputCollection[1].OutputColumnCollection.AddOutputColumns(ComponentMetaData.InputCollection[0].InputColumnCollection);
                    break;
            }
            return base.SetComponentProperty(propertyName, propertyValue);
        }

        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);
            if (this.ComponentMetaData.InputCollection[0].IsAttached &&
                this.ComponentMetaData.InputCollection[1].IsAttached)
            {
                DefineOutputs();
            }
        }

        private IList<string> GetComparisonColumnKeys(bool fireInformationMessages = false)
        {
            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var rightCols = ComponentMetaData.InputCollection[1].InputColumnCollection;
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
            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var rightCols = ComponentMetaData.InputCollection[1].InputColumnCollection;
            var commonDefs = GetComparisonColumnKeys(true);
            var matchedOutputColumns = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            matchedOutputColumns.RemoveAll();
            matchedOutputColumns.AddOutputColumns(leftCols);
            for (int z = 0; z < rightCols.Count; ++z)
            {
                var col = rightCols[z];
                if (!commonDefs.Contains(CreateColumnFingerprint(col)))
                {
                    matchedOutputColumns.AddOutputColumn(col);
                }
            }
            var orphanOutputColumns = ComponentMetaData.OutputCollection[1].OutputColumnCollection;
            orphanOutputColumns.RemoveAll();
            orphanOutputColumns.AddOutputColumns(leftCols);
        }

        public override DTSValidationStatus Validate()
        {
            var ret = base.Validate();
            switch (ret)
            {
                case DTSValidationStatus.VS_ISVALID:
                    if (!ComponentMetaData.InputCollection[0].IsAttached || !ComponentMetaData.InputCollection[1].IsAttached)
                    {
                        ret = DTSValidationStatus.VS_ISBROKEN;
                    }
                    else
                    {
                        var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
                        var rightCols = ComponentMetaData.InputCollection[1].InputColumnCollection;
                        var commonDefs = GetComparisonColumnKeys();
                        var matchedOutputColumns = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
                        var orphanOutputColumns = ComponentMetaData.OutputCollection[1].OutputColumnCollection;
                        if (matchedOutputColumns.Count != leftCols.Count + rightCols.Count - commonDefs.Count)
                        {
                            ret = DTSValidationStatus.VS_NEEDSNEWMETADATA;
                        }
                        else if (orphanOutputColumns.Count != leftCols.Count)
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

        private ColumnBufferMapping InputRootBufferColumnIndicees;
        private ColumnBufferMapping InputComparisonBufferColumnIndicees;
        private ColumnBufferMapping OutputMatchesBufferColumnIndicees;
        private ColumnBufferMapping OutputOrphansBufferColumnIndicees;

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
            InputRootBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.InputCollection[0]);
            InputComparisonBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.InputCollection[1]);
            OutputMatchesBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.OutputCollection[0]);
            OutputOrphansBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.OutputCollection[1]);
            GetComparisonColumnKeys(true);
        }

        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
            switch (input.Name)
            {
                case PropertyNames.InputProperties.Root:
                    if (!InputRootProcessed)
                    {
                        ProcessLeftInput(input, buffer);
                    }
                    break;
                case PropertyNames.InputProperties.Comparison:
                    if (!InputComparisonProcessed)
                    {
                        ProcessRightInput(input, buffer);
                    }
                    break;
                default:
                    bool fireAgain = true;
                    ComponentMetaData.FireInformation(0, "", string.Format("Not expecting inputID={0}", inputID), "", 0, ref fireAgain);
                    throw new InvalidOperationException(string.Format("Not expecting inputID={0}", inputID));
            }
        }


        private int InputFingerprintsSampled;
        private int ProcessInputRootHits = 0;
        private int ProcessInputRootMisses = 0;
        private int ProcessInputRootFanoutHits = 0;

        private void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            var matchAttached = ComponentMetaData.OutputCollection[0].IsAttached;
            var matchedCC = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            var orphansAttached = ComponentMetaData.OutputCollection[1].IsAttached;
            var orhpannedCC = ComponentMetaData.OutputCollection[1].OutputColumnCollection;
            var commonFingerprints = GetComparisonColumnKeys();
            var fingerprinter = new Fingerprinter();
            var sourceVals = new List<object>();
            int rowsProcessed = 0;
            while (buffer.NextRow())
            {
                for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                {
                    var col = input.InputColumnCollection[z];
                    var colFingerprint = CreateColumnFingerprint(col);
                    var o = GetObject(col.Name, buffer, InputRootBufferColumnIndicees);
                    if (commonFingerprints.Contains(colFingerprint))
                    {
                        fingerprinter.Include(col.Name, o);
                    }
                    sourceVals.Add(o);
                }
                var fingerprint = fingerprinter.GetFingerPrint();
                if (AppendsByCommonFieldHash.ContainsKey(fingerprint))
                {
                    ++ProcessInputRootHits;
                    if (matchAttached)
                    {
                        foreach (var appends in AppendsByCommonFieldHash[fingerprint])
                        {
                            ++ProcessInputRootFanoutHits;
                            CopyToMatchedOutput(matchedCC, sourceVals, appends);
                        }
                    }
                }
                else
                {
                    ++ProcessInputRootMisses;
                    if (orphansAttached)
                    {
                        CopyToOrphannedOutput(orhpannedCC, sourceVals);
                    }
                }
                fingerprinter.Clear();
                sourceVals.Clear();
                ++rowsProcessed;
                if (InputFingerprintsSampled < SampleSize)
                {
                    ++InputFingerprintsSampled;
                    FireInformation(InformationMessageCodes.ExampleFingerprint, fingerprint);
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
                MatchedOutputBuffer.SetEndOfRowset();
                OrphannedOutputBuffer.SetEndOfRowset();
                InputRootProcessed = true;
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
                    case PropertyNames.InputProperties.RootId:
                        can = InputRootProcessed || InputComparisonProcessed;
                        break;
                    case PropertyNames.InputProperties.ComparisonId:
                        can = true; //!InputComparisonProcessed;
                        break;
                    default:
                        can = false;
                        break;
                }
                canProcess[i] = can;
            }
        }

        private void CopyToMatchedOutput(IDTSOutputColumnCollection100 cc, IList<object> sources, IList<object> appends)
        {
            var buf = MatchedOutputBuffer;
            buf.AddRow();
            CopyValsToBuffer(buf, cc, sources, 0, OutputMatchesBufferColumnIndicees);
            CopyValsToBuffer(buf, cc, appends, sources.Count, OutputMatchesBufferColumnIndicees);
        }

        private void CopyToOrphannedOutput(IDTSOutputColumnCollection100 cc, IList<object> sources)
        {
            var buf = OrphannedOutputBuffer;
            buf.AddRow();
            CopyValsToBuffer(buf, cc, sources, 0, OutputOrphansBufferColumnIndicees);
        }

        private void CopyValsToBuffer(PipelineBuffer buf, IDTSOutputColumnCollection100 cc, IList<object> vals, int offset, ColumnBufferMapping cbm)
        {
            for (int z = 0; z < vals.Count; ++z)
            {
                var col = cc[z + offset];
                var i = cbm.PositionByColumnPosition[z + offset];
                var o = vals[z];
                buf.SetObject(col.DataType, i, o);
            }
        }

        PipelineBuffer MatchedOutputBuffer;
        PipelineBuffer OrphannedOutputBuffer;

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            if (buffers.Length == 2)
            {
                MatchedOutputBuffer = buffers[0];
                OrphannedOutputBuffer = buffers[1];
            }
        }

        public override void PrepareForExecute()
        {
            base.PrepareForExecute();
            InputRootProcessed = false;
            InputComparisonProcessed = false;
        }

        private MultipleValueDictionary<string, object[]> AppendsByCommonFieldHash;
        private bool InputComparisonProcessed = false;
        private bool InputRootProcessed = false;
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
                    var o = GetObject(col.Name, buffer, InputComparisonBufferColumnIndicees);
                    if (commonFingerprints.Contains(colFingerprint))
                    {
                        fingerprinter.Include(col.Name, o);
                    }
                    else
                    {
                        appends.Add(o);
                    }
                }
                var fingerprint = fingerprinter.GetFingerPrint();
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
            InputComparisonProcessed = buffer.EndOfRowset;
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
