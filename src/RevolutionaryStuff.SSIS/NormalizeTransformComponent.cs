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
    [DtsPipelineComponent(
        DisplayName = "Normalize",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = true,
        IconResource = "RevolutionaryStuff.SSIS.Resources.Icon1.ico")]
    public class NormalizeTransformComponent : BasePipelineComponent
    {
        private static class PropertyNames
        {
            public const string Mappings = "Mappings";
            public const string OutputColumnNames = "OutputColumnNames";
        }

        private const char StringConstantPrefix = ':';
        private const char IntConstantPrefix = '=';

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "Normalize";
            ComponentMetaData.Description = "Turns groups of columns into disparate rows.";

            var left = ComponentMetaData.InputCollection.New();
            left.Name = "Denormalized";
            var outs = ComponentMetaData.OutputCollection.New();
            outs.SynchronousInputID = 0;
            outs.Name = "Normalized";

            CreateCustomProperty(PropertyNames.Mappings, "", "Each tuple becomes a row;  The output data types will match those of the first row; => colA,colB,colC;colM,colN,colO;");
            CreateCustomProperty(PropertyNames.OutputColumnNames, "", "Output column names;  colA,colB,colC");
        }

        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);
            DefineOutputs();
        }

        IList<IList<string>> GetMappings()
        {
            var ret = new List<IList<string>>();
            var csv = (GetCustomPropertyAsString(PropertyNames.Mappings) ?? "").Replace(";", "\n");
            foreach (var row in CSV.ParseText(csv))
            {
                var cols = row.Select(z => StringHelpers.TrimOrNull(z)).ToList().AsReadOnly();
                ret.Add(cols);
            }
            return ret;
        }

        private bool IsLiteralColumn(string colName)
            => colName.StartsWith(":")|| colName.StartsWith("=");

        IList<string> GetOutputColumnNames()
            => CSV.ParseLine(GetCustomPropertyAsString(PropertyNames.OutputColumnNames) ?? "").Select(z => StringHelpers.TrimOrNull(z)).Where(z => z != null).ToList();

        private void DefineOutputs()
        {
            if (!ComponentMetaData.InputCollection[0].IsAttached) return;

            var m = GetMappings();

            var allInputColumnNames = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            foreach (var r in m)
            {
                foreach (var c in r)
                {
                    if (!IsLiteralColumn(c))
                    {
                        allInputColumnNames.Add(StringHelpers.TrimOrNull(c));
                    }
                }
            }
            var virtualInputs = ComponentMetaData.InputCollection[0].GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 vcol in virtualInputs.VirtualInputColumnCollection)
            {
                if (allInputColumnNames.Contains(vcol.Name))
                {
                    virtualInputs.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
                }
            }

            var inputColumnsByName = new Dictionary<string, IDTSInputColumn100>(Comparers.CaseInsensitiveStringComparer);
            foreach (IDTSInputColumn100 c in ComponentMetaData.InputCollection[0].InputColumnCollection)
            {
                inputColumnsByName[c.Name] = c;
            }
            var inNames = m[0];
            var outNames = GetOutputColumnNames();

            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            outCols.RemoveAll();
            for (int z = 0; z < inNames.Count; ++z)
            {
                var outName = outNames[z];
                var inName = inNames[z];
                if (IsLiteralColumn(inName))
                {
                    var outCol = outCols.New();
                    outCol.Name = outName;
                    var t = inName[0];
                    switch (t)
                    {
                        case IntConstantPrefix:
                            outCol.SetDataTypeProperties(DataType.DT_I4, 0, 0, 0, 0);
                            break;
                        case StringConstantPrefix:
                            outCol.SetDataTypeProperties(DataType.DT_WSTR, 80, 0, 0, 0);
                            break;
                        default:
                            throw new UnexpectedSwitchValueException(t);
                    }
                }
                else
                {
                    var inCol = inputColumnsByName[inName];
                    outCols.AddOutputColumn(inCol, outName);
                }
            }
        }

        public override DTSValidationStatus Validate()
        {
            var ret = base.Validate();
            switch (ret)
            {
                case DTSValidationStatus.VS_ISVALID:
                    if (!ComponentMetaData.InputCollection[0].IsAttached)
                    {
                        ret = DTSValidationStatus.VS_ISBROKEN;
                    }
                    else
                    {
                        var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
                        var inNames = GetMappings()[0];
                        var outNames = GetOutputColumnNames();
                        if (inNames.Count != outNames.Count || outCols.Count != outNames.Count)
                        {
                            ret = DTSValidationStatus.VS_ISBROKEN;
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

        private int RowsProcessed;
        private ColumnBufferMapping InputCbm;
        private ColumnBufferMapping OutputCbm;

        public override void PreExecute()
        {
            base.PreExecute();
            InputCbm = GetBufferColumnIndicees(ComponentMetaData.InputCollection[0]);
            OutputCbm = GetBufferColumnIndicees(ComponentMetaData.OutputCollection[0]);
        }

        PipelineBuffer OutputBuffer;

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            if (buffers.Length == 1)
            {
                OutputBuffer = buffers[0];
            }
        }

        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var m = GetMappings();
            var outNames = GetOutputColumnNames();

            var sampleOuts = new List<object>();
            while (buffer.NextRow())
            {
                foreach (var row in m)
                {
                    sampleOuts.Clear();
                    OutputBuffer.AddRow();
                    for (int z = 0; z < row.Count; ++z)
                    {
                        var inColName = row[z];
                        object o;
                        if (IsLiteralColumn(inColName))
                        {
                            var sval = inColName.Substring(1);
                            var t = inColName[0];
                            switch (t)
                            {
                                case IntConstantPrefix:
                                    o = int.Parse(sval);
                                    break;
                                case StringConstantPrefix:
                                    o = sval;
                                    break;
                                default:
                                    throw new UnexpectedSwitchValueException(t);
                            }
                        }
                        else
                        {
                            o = GetObject(inColName, buffer, InputCbm);
                        }
                        if (RowsProcessed < SampleSize)
                        {
                            sampleOuts.Add(o);
                        }
                        var outColName = outNames[z];
                        OutputBuffer.SetObject(outColName, OutputCbm, o);
                    }
                    if (RowsProcessed < SampleSize)
                    {
                        this.FireInformation(InformationMessageCodes.Sample, CSV.FormatLine(sampleOuts, false));
                    }
                }
                ++RowsProcessed;
            }
            if (buffer.EndOfRowset)
            {
                OutputBuffer.SetEndOfRowset();
            }
        }

        public override void IsInputReady(int[] inputIDs, ref bool[] canProcess)
        {
            for (int i = 0; i < inputIDs.Length; i++)
            {
                canProcess[i] = true;
            }
        }


        private enum InformationMessageCodes
        {
            Sample = 1,
        }
    }
}
