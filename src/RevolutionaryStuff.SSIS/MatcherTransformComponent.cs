using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using RevolutionaryStuff.Core.Collections;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "The Matcher", 
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = true,
        IconResource = "RevolutionaryStuff.SSIS.Resources.Icon1.ico")]
    public class MatcherTransformComponent : BasePipelineComponent
    {
        private static class PropertyNames
        {
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

            ComponentMetaData.Name = "The Matcher";
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
        }

        public override IDTSCustomProperty100 SetComponentProperty(string propertyName, object propertyValue)
        {
            switch (propertyName)
            {
                case PropertyNames.OutputProperties.Matches:
                    AddOutputColumns(ComponentMetaData.InputCollection[0].InputColumnCollection, ComponentMetaData.OutputCollection[0].OutputColumnCollection);
                    break;
                case PropertyNames.OutputProperties.Orphans:
                    AddOutputColumns(ComponentMetaData.InputCollection[0].InputColumnCollection, ComponentMetaData.OutputCollection[1].OutputColumnCollection);
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


        private IList<string> GetComparisonColumnKeys(bool fireInformationMessages=false)
        {
            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var rightCols = ComponentMetaData.InputCollection[1].InputColumnCollection;
            var leftDefs = new HashSet<string>();
            var rightDefs = new HashSet<string>();
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
            var commonDefs = new HashSet<string>(leftDefs.Intersect(rightDefs));
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
            var commonDefs = GetComparisonColumnKeys();
            var matchedOutputColumns = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            matchedOutputColumns.RemoveAll();
            AddOutputColumns(leftCols, matchedOutputColumns);
            for (int z = 0; z < rightCols.Count; ++z)
            {
                var col = rightCols[z];
                if (!commonDefs.Contains(CreateColumnFingerprint(col)))
                {
                    CopyColumnDefinition(matchedOutputColumns, col);
                }
            }
            var orphanOutputColumns = ComponentMetaData.OutputCollection[1].OutputColumnCollection;
            orphanOutputColumns.RemoveAll();
            AddOutputColumns(leftCols, orphanOutputColumns);
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
            InputRootBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.InputCollection[0]);
            InputComparisonBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.InputCollection[1]);
            OutputMatchesBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.OutputCollection[0]);
            OutputOrphansBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.OutputCollection[1]);
            GetComparisonColumnKeys(true);
        }

        public override void ProcessInput(int inputID, PipelineBuffer buffer)
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
                    var o = GetObject(col.Name, col.DataType, z, buffer, InputComparisonBufferColumnIndicees);
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
                if (InputFingerprintsSampled < FingerprintSampleSize)
                {
                    ++InputFingerprintsSampled;
                    FireInformation(InformationMessageCodes.ExampleFingerprint, fingerprint);
                }
                if (rowsProcessed % 100 == 0)
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
                var i = cbm.ByColumnPosition[z+offset];
                var o = vals[z];
                SetObject(buf, col.DataType, i, o);
            }            
        }

        private static void SetObject(PipelineBuffer buf, DataType dt, int i, object val)
        {
            if (val == null)
            {
                buf.SetNull(i);
            }
            else
            {
                switch (dt)
                {
                    case DataType.DT_GUID:
                        buf.SetGuid(i, (Guid)val);
                        break;
                    case DataType.DT_BOOL:
                        buf.SetBoolean(i, (bool)val);
                        break;
                    case DataType.DT_UI2:
                        buf.SetUInt16(i, (System.UInt16)val);
                        break;
                    case DataType.DT_UI4:
                        buf.SetUInt32(i, (System.UInt32)val);
                        break;
                    case DataType.DT_UI8:
                        buf.SetUInt64(i, (System.UInt64)val);
                        break;
                    case DataType.DT_I1:
                        buf.SetByte(i, (byte)val);
                        break;
                    case DataType.DT_I2:
                        buf.SetInt16(i, (System.Int16)val);
                        break;
                    case DataType.DT_I4:
                        buf.SetInt32(i, (System.Int32)val);
                        break;
                    case DataType.DT_I8:
                        buf.SetInt64(i, (System.Int64)val);
                        break;
                    case DataType.DT_WSTR:
                    case DataType.DT_STR:
                    case DataType.DT_TEXT:
                        buf.SetString(i, (string)val);
                        break;
                    case DataType.DT_DATE:
                        buf.SetDate(i, (DateTime)val);
                        break;
                    case DataType.DT_DECIMAL:
                        buf.SetDecimal(i, (decimal)val);
                        break;
                    default:
                        buf.SetNull(i);
                        break;
                }
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
            AppendsByCommonFieldHash.Clear();
            InputRootProcessed = false;
            InputComparisonProcessed = false;
        }

        private MultipleValueDictionary<string, object[]> AppendsByCommonFieldHash = new MultipleValueDictionary<string, object[]>();
        private bool InputComparisonProcessed = false;
        private bool InputRootProcessed = false;
        private int ComparisonFingerprintsSampled = 0;
        private const int FingerprintSampleSize = 10;

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
                    var o = GetObject(col.Name, col.DataType, z, buffer, InputComparisonBufferColumnIndicees);
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
                if (ComparisonFingerprintsSampled < FingerprintSampleSize)
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
            RowsProcessed=1,
            ExampleFingerprint=2,
            AppendsByCommonFieldHash=3,
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
            def = col.Name + ";" + def;
            return def.ToLower();
        }
#endregion
    }
}
