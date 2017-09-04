﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "The Mapper",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = true,
        IconResource = "RevolutionaryStuff.SSIS.Resources.Icon1.ico")]
    public class MapperTransformComponent : BasePipelineComponent
    {
        private static class PropertyNames
        {
            public const string IgnoreCase = CommonPropertyNames.IgnoreCase;
            public const string Mappings = "Mappings";

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
            }
        }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "The Mapper";
            ComponentMetaData.Description = "A SSIS Data Flow Transformation Component that maps keys from the right table onto the left based off of join conditions.";

            CreateCustomProperty(PropertyNames.Mappings, null, "Lookup1ResultFieldName,Lookup1Key1,...Lookup1KeyN;LookupNResultFieldName,LookupNKey1,...LookupNKeyN;");
            CreateCustomProperty(PropertyNames.IgnoreCase, "1", "When {1,true} the match is case insensitive, when {0,false} it is case sensitive.");

            var left = ComponentMetaData.InputCollection.New();
            left.Name = PropertyNames.InputProperties.Root;
            var right = ComponentMetaData.InputCollection.New();
            right.Name = PropertyNames.InputProperties.Comparison;
            var matched = ComponentMetaData.OutputCollection.New();
            matched.SynchronousInputID = 0;
            matched.Name = PropertyNames.OutputProperties.Matches;
            matched.Description = "Root rows that have have corresponding matches in the Comparison";
        }
        
        IDictionary<string, IList<string>> ParseMappings()
        {
            var p = ComponentMetaData.CustomPropertyCollection[PropertyNames.Mappings];
            var d = new Dictionary<string, IList<string>>();
            var csv = (p.Value as string ?? "").Replace(";", "\n");
            foreach (var row in CSV.ParseText(csv))
            {
                if (row.Length < 2) continue;
                d[row[0]] = new List<string>(row.Skip(1)).AsReadOnly();
            }
            return d;
        }

        public override IDTSCustomProperty100 SetComponentProperty(string propertyName, object propertyValue)
        {
            switch (propertyName)
            {
                case PropertyNames.OutputProperties.Matches:
                    ComponentMetaData.OutputCollection[0].OutputColumnCollection.AddOutputColumns(ComponentMetaData.InputCollection[0].InputColumnCollection);
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
            var rightKeyCol = ComponentMetaData.InputCollection[1].InputColumnCollection[0];
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            outCols.RemoveAll();
            outCols.AddOutputColumns(leftCols);
            var m = ParseMappings();
            foreach (var key in m.Keys)
            {
                outCols.AddOutputColumn(rightKeyCol, key);
            }
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
                        var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
                        var m = ParseMappings();
                        if (outCols.Count != leftCols.Count + m.Count)
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
        private ColumnBufferMapping OutputBufferColumnIndicees;

        public override void PreExecute()
        {
            base.PreExecute();
            if (GetCustomPropertyAsBool(PropertyNames.IgnoreCase, true))
            {
                ValByKey = new Dictionary<string, object>(Comparers.CaseInsensitiveStringComparer);
            }
            else
            {
                ValByKey = new Dictionary<string, object>();
            }
            InputRootBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.InputCollection[0]);
            InputComparisonBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.InputCollection[1]);
            OutputBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.OutputCollection[0]);
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

        private int ProcessInputRootHits = 0;
        private int ProcessInputRootMisses = 0;

        private void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            if (!ComponentMetaData.OutputCollection[0].IsAttached) return;
            var m = ParseMappings();

            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var rightKeyCol = ComponentMetaData.InputCollection[1].InputColumnCollection[0];
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            var outputBuffer = MatchedOutputBuffer;

            int rowsProcessed = 0;
            var keys = new List<object>();
            while (buffer.NextRow())
            {
                outputBuffer.AddRow();
                for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                {
                    var col = input.InputColumnCollection[z];
                    var o = GetObject(col.Name, col.DataType, z, buffer, InputRootBufferColumnIndicees);
                    outputBuffer.SetObject(col.DataType, OutputBufferColumnIndicees.PositionByColumnName[col.Name], o);
                }
                foreach (var kvp in m)
                {
                    keys.Clear();
                    foreach (var fn in kvp.Value)
                    {
                        object o;
                        if (fn.StartsWith(":"))
                        {
                            o = fn.Substring(1);
                        }
                        else
                        {
                            o = GetObject(fn, buffer, InputRootBufferColumnIndicees);
                        }
                        keys.Add(o);
                    }
                    var key = Cache.CreateKey(keys);
                    object val;
                    if (ValByKey.TryGetValue(key, out val))
                    {
                        ++ProcessInputRootHits;
                    }
                    else
                    {
                        ++ProcessInputRootMisses;
                        val = null;
                    }
                    outputBuffer.SetObject(rightKeyCol.DataType, OutputBufferColumnIndicees.PositionByColumnName[kvp.Key], val);
                }
                ++rowsProcessed;
            }
            FireInformation(InformationMessageCodes.RowsProcessed, $"{rowsProcessed}");
            FireInformation(InformationMessageCodes.MatchStats, $"hits={ProcessInputRootHits}, misses={ProcessInputRootMisses}");
            if (buffer.EndOfRowset)
            {
                MatchedOutputBuffer.SetEndOfRowset();
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

        PipelineBuffer MatchedOutputBuffer;

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            if (buffers.Length == 1)
            {
                MatchedOutputBuffer = buffers[0];
            }
        }

        public override void PrepareForExecute()
        {
            base.PrepareForExecute();
            ValByKey.Clear();
            InputRootProcessed = false;
            InputComparisonProcessed = false;
        }

        private bool InputComparisonProcessed = false;
        private bool InputRootProcessed = false;
        private int ComparisonFingerprintsSampled = 0;
        private const int FingerprintSampleSize = 10;
        private IDictionary<string, object> ValByKey;

        private void ProcessRightInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            int rowsProcessed = 0;
            var keys = new List<object>();
            while (buffer.NextRow())
            {
                keys.Clear();
                var idCol = input.InputColumnCollection[0];
                var idVal = GetObject(idCol.Name, idCol.DataType, 0, buffer, InputComparisonBufferColumnIndicees);
                for (int z = 1; z < input.InputColumnCollection.Count; ++z)
                {
                    var col = input.InputColumnCollection[z];
                    var o = GetObject(col.Name, buffer, InputComparisonBufferColumnIndicees);
                    keys.Add(o);
                }
                var key = Cache.CreateKey(keys);
                ValByKey[key] = idVal;
                ++rowsProcessed;
                if (ComparisonFingerprintsSampled < FingerprintSampleSize)
                {
                    ++ComparisonFingerprintsSampled;
                    FireInformation(InformationMessageCodes.ExampleFingerprint, $"{idVal}<={key}");
                }
            }
            FireInformation(InformationMessageCodes.RowsProcessed, $"{rowsProcessed}");
            InputComparisonProcessed = buffer.EndOfRowset;
        }

        private enum InformationMessageCodes
        {
            RowsProcessed = 1,
            ExampleFingerprint = 2,
            MatchStats = 4,
            CommonColumns = 5,
            LeftColumns = 6,
            RightColumns = 7,
        }
    }
}
