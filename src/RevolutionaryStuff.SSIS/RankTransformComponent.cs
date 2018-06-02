using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using RevolutionaryStuff.Core;
using System.Text;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "Rank",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = true,
        NoEditor = false,
        CurrentVersion = BasePipelineComponent.AssemblyComponentVersion,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class RankTransformComponent : BasePipelineComponent
    {
        private class OrderedColumn
        {
            public bool IsAscending { get; }
            public string ColumnName { get; }
            public bool IsValid { get; }
            public OrderedColumn(string term)
            {
                if (term == null || term.Length < 3) return;
                term.Trim();
                IsAscending = char.ToLower(term[0]) == 'd';
                ColumnName = term.Substring(2).Trim();
                IsValid = true;
            }
        }

        private static class PropertyNames
        {
            public const string IgnoreCase = CommonPropertyNames.IgnoreCase;
            public const string ParitionByFields = "PartitionBy";
            public const string OrderByClause = "OrderBy";
            public const string RankFieldName = "RankFieldName";
            public const string ReturnOnlyRank1 = "ReturnOnlyRank1";

            public static class InputProperties
            {
                public const int TheInputId = 0;
                public const string TheInputName = "Right Input";
            }

            public static class OutputProperties
            {
                public const int TheOutputId = 0;
            }
        }

        IDTSInput100 TheInput => ComponentMetaData.InputCollection[PropertyNames.InputProperties.TheInputId];
        IDTSInputColumnCollection100 TheInputColumns => TheInput.InputColumnCollection;
        IDTSOutput100 TheOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.TheOutputId];
        IDTSOutputColumnCollection100 TheOutputColumns => TheOutput.OutputColumnCollection;

        IList<string> GetPartitionColumnNames()
            => CSV.ParseLine(GetCustomPropertyAsString(PropertyNames.ParitionByFields) ?? "").Select(z => StringHelpers.TrimOrNull(z)).Where(z => z != null).ToList().AsReadOnly();

        IList<OrderedColumn> GetOrderedColumns()
            => CSV.ParseLine(GetCustomPropertyAsString(PropertyNames.OrderByClause) ?? "").Select(z => new OrderedColumn(z)).Where(z => z.IsValid).ToList().AsReadOnly();

        string RankFieldName
            => StringHelpers.TrimOrNull(GetCustomPropertyAsString(PropertyNames.RankFieldName));

        bool HasRankField
            => RankFieldName != null;

        public RankTransformComponent()
            : base(false)
        { }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "Rank";
            ComponentMetaData.Description = "Performs a database like 'rank' operation.";

            var input = ComponentMetaData.InputCollection.New();
            input.Name = PropertyNames.InputProperties.TheInputName;

            var output = ComponentMetaData.OutputCollection.New();
            output.SynchronousInputID = 0;
            output.Name = "The Output";

            CreateCustomProperty(PropertyNames.IgnoreCase, "1", "When {1,true} the match is case insensitive, when {0,false} it is case sensitive.");
            CreateCustomProperty(PropertyNames.ParitionByFields, "", "A csv list of the fields on which we are paritioning: [Field1,Field5,Field3]");
            CreateCustomProperty(PropertyNames.OrderByClause, "", "A csv list of fields that we're ordering by for the rank within the parition with a prefix of [A:] for ascending or [D:] for descending: [A:Field2,D:Field8]");
            CreateCustomProperty(PropertyNames.RankFieldName, "R", "The name of the the field in which the rank will be stored.  When empty, do not include the rank as an output column.");
            CreateCustomProperty(PropertyNames.ReturnOnlyRank1, "0", "When {1,true} only rows with rank=1 are returned;  When {0,false} all rows are returned");            
        }

        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);
            DefineOutputs();
        }

        public override void ReinitializeMetaData()
        {
            base.ReinitializeMetaData();
            DefineOutputs();
        }

        private void DefineOutputs()
        {
            if (!TheInput.IsAttached) return;
            var input = ComponentMetaData.InputCollection[PropertyNames.InputProperties.TheInputId].GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                input.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
            }

            var inputColumns = TheInputColumns;
            var outputColumns = TheOutputColumns;
            outputColumns.RemoveAll();
            for (int z = 0; z < inputColumns.Count; ++z)
            {
                var col = inputColumns[z];
                outputColumns.AddOutputColumn(col);
            }
            if (HasRankField)
            {
                var outCol = outputColumns.New();
                outCol.Name = RankFieldName;
                outCol.SetDataTypeProperties(DataType.DT_I4, 0, 0, 0, 0);
            }
        }

        protected override DTSValidationStatus OnValidate()
        {
            var ret = base.OnValidate();
            if (ret != DTSValidationStatus.VS_ISVALID) return ret;
            if (!TheInput.IsAttached)
            {
                return DTSValidationStatus.VS_ISBROKEN;
            }
            else
            {
                var rightCols = TheInputColumns;
                if (TheOutputColumns.Count != (TheInputColumns.Count + (HasRankField ? 1 : 0)))
                {
                    ret = DTSValidationStatus.VS_NEEDSNEWMETADATA;
                }
                else
                {
                    for (int z = 0; z < TheInputColumns.Count; ++z)
                    {
                        var inc = TheInputColumns[z];
                        var outc = TheOutputColumns[z];
                        if (inc.Name != outc.Name || inc.DataType != outc.DataType)
                        {
                            ret = DTSValidationStatus.VS_NEEDSNEWMETADATA;
                        }
                    }
                }
            }
            return DTSValidationStatus.VS_ISVALID;
        }

        protected override RuntimeData ConstructRuntimeData()
            => new MyRuntimeData(this);

        private new MyRuntimeData RD
            => (MyRuntimeData)base.RD;

        protected override void OnProcessInput(int inputId, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputId);
            switch (input.Name)
            {
                case PropertyNames.InputProperties.TheInputName:
                    ProcessRightInput(input, buffer);
                    break;
                default:
                    bool fireAgain = true;
                    ComponentMetaData.FireInformation(0, "", string.Format("Not expecting inputID={0}", inputId), "", 0, ref fireAgain);
                    throw new InvalidOperationException(string.Format("Not expecting inputID={0}", inputId));
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
                    case PropertyNames.InputProperties.TheInputId:
                        can = true; //!InputComparisonProcessed;
                        break;
                    default:
                        can = false;
                        break;
                }
                canProcess[i] = can;
            }
        }

        PipelineBuffer TheOutputBuffer;

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            using (CreateTraceRegion($"outputs={outputs}"))
            {
                base.PrimeOutput(outputs, outputIDs, buffers);
                if (buffers.Length == 1)
                {
                    TheOutputBuffer = buffers[PropertyNames.OutputProperties.TheOutputId];
                }
            }
        }

        private int ComparisonFingerprintsSampled = 0;
        private int InputRowsProcessed = 0;

        private void ProcessRightInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            while (buffer.NextRow())
            {
                var rd = RD.P.Add(input, buffer);
                ++InputRowsProcessed;
                if (ComparisonFingerprintsSampled < SampleSize)
                {
                    ++ComparisonFingerprintsSampled;
                    FireInformation(InformationMessageCodes.ExampleFingerprint, rd.PartitionKey);
                }
            }
            FireInformation(InformationMessageCodes.InputRowsProcessed, $"{InputRowsProcessed}");
        }

        protected override void OnProcessInputEndOfRowset(int inputID)
        {
            base.OnProcessInputEndOfRowset(inputID);
            if (TheOutput.IsAttached)
            {
                using (CreateTraceRegion("Sorting so we can rank"))
                {
                    RD.P.Sort();
                }
                string lastPk = null;
                int r = 0;
                int distinctPartitionKeys = 0;
                int rankGreaterOneSkips = 0;
                bool only1 = GetCustomPropertyAsBool(PropertyNames.ReturnOnlyRank1, false);
                var cnt = RD.P.Rows.Count;
                for (int rowsProcessed = 0; rowsProcessed < cnt; ++rowsProcessed)
                {
                    var row = RD.P.Rows[rowsProcessed];
                    if (row.PartitionKey != lastPk)
                    {
                        r = 0;
                        ++distinctPartitionKeys;
                        lastPk = row.PartitionKey;
                    }
                    if (++r > 1 && only1)
                    {
                        ++rankGreaterOneSkips;
                        continue;
                    }
                    RD.P.Emit(row, TheOutputBuffer, RD.TheOutputCbm, r);
                    if (rowsProcessed % StatusNotifyIncrement == 0 || rowsProcessed==cnt-1)
                    {
                        FireInformation(InformationMessageCodes.RankStats, $"rowsProcessed={rowsProcessed}, distinctPartitionKeys={distinctPartitionKeys}, rankGreaterOneSkips={rankGreaterOneSkips}");
                    }
                }
            }
            TheOutputBuffer.SetEndOfRowset();
        }

        private enum InformationMessageCodes
        {
            InputRowsProcessed = 1,
            ExampleFingerprint = 2,
            CommonColumns = 5,
            LeftColumns = 6,
            RightColumns = 7,
            SortStatus = 8,
            RankStats = 9,
        }

        private class MyRuntimeData : RuntimeData
        {
            protected new RankTransformComponent Parent
                => (RankTransformComponent)base.Parent;

            public readonly bool IgnoreCase;
            public readonly ColumnBufferMapping TheInputCbm;
            public readonly ColumnBufferMapping TheOutputCbm;
            public readonly Purgatory P;

            public MyRuntimeData(RankTransformComponent parent)
                : base(parent)
            {
                IgnoreCase = GetCustomPropertyAsBool(PropertyNames.IgnoreCase);
                TheInputCbm = InputColumnBufferMappings[0];
                TheOutputCbm = OutputColumnBufferMappings[0];
                P = new Purgatory(Parent, TheInputCbm, Parent.GetPartitionColumnNames(), Parent.GetOrderedColumns(), IgnoreCase);
            }
        }

        private class Purgatory
        {
            private readonly RankTransformComponent Parent;
            private readonly ColumnBufferMapping Cbm;
            private readonly IList<string> PartitionColumnNames;
            private readonly IList<OrderedColumn> OrderedColumns;
            public readonly List<RowData> Rows = new List<RowData>();
            private readonly string RankFieldName;
            private readonly bool HasRankField;
            private readonly bool IgnoreCase;

            public class RowData
            {
                public string PartitionKey;
                public object[] SortVals;
                public object[] AllValsByInputPos;
            }

            internal Purgatory(RankTransformComponent component, ColumnBufferMapping cbm, IList<string> partitionColumnNames, IList<OrderedColumn> orderedColumns, bool ignoreCase)
            {
                Parent = component;
                Cbm = cbm;
                PartitionColumnNames = partitionColumnNames;
                OrderedColumns = orderedColumns;
                RankFieldName = component.RankFieldName;
                HasRankField = component.HasRankField;
                IgnoreCase = ignoreCase;
            }

            public void Emit(RowData row, PipelineBuffer outputBuffer, ColumnBufferMapping outputCbm, int rank)
            {
                outputBuffer.AddRow();
                for (int z = 0; z < this.Cbm.ColumnCount; ++z)
                {
                    var val = row.AllValsByInputPos[z];
                    var name = Cbm.GetColumnNameFromPosition(z);
                    outputBuffer.SetObject(name, outputCbm, val);
                }
                if (HasRankField)
                {
                    outputBuffer.SetObject(RankFieldName, outputCbm, rank);
                }
            }

            public void Sort()
                =>Rows.Sort((l, r) => 
                {
                    var ret = l.PartitionKey.CompareTo(r.PartitionKey);
                    if (ret == 0)
                    {
                        for (int z = 0; z < OrderedColumns.Count; ++z)
                        {
                            var oc = OrderedColumns[z];
                            var a = l.SortVals[z] as IComparable;
                            var b = r.SortVals[z];
                            if (a == b) continue;
                            if (a == null)
                            {
                                ret = -1;
                            }
                            else if (b == null)
                            {
                                ret = 1;
                            }
                            else
                            {
                                ret = a.CompareTo(b);
                            }
                            if (ret == 0) continue;
                            ret = ret * (oc.IsAscending ? 1 : -1);
                            break;
                        }
                    }
                    return ret;
                });

            private readonly StringBuilder PkSb = new StringBuilder();
            public RowData Add(IDTSInput100 input, PipelineBuffer buffer)
            {
                PkSb.Clear();
                var rd = new RowData()
                {
                    AllValsByInputPos = new object[Cbm.ColumnCount],
                    SortVals = new object[OrderedColumns.Count]
                };
                foreach (var name in PartitionColumnNames)
                {
                    var v = Parent.GetObject(name, buffer, Cbm);
                    PkSb.Append($"{v};");
                }
                rd.PartitionKey = IgnoreCase ? PkSb.ToString().ToLower() : PkSb.ToString();
                int pos = 0;
                foreach (var oc in OrderedColumns)
                {
                    var v = Parent.GetObject(oc.ColumnName, buffer, Cbm);
                    rd.SortVals[pos++] = v;
                }
                for (int z = 0; z < Cbm.ColumnCount; ++z)
                {
                    var v = Parent.GetObject(Cbm.GetColumnNameFromPosition(z), buffer, Cbm);
                    rd.AllValsByInputPos[z] = v;
                }
                Rows.Add(rd);
                return rd;
            }
        }
    }
}
