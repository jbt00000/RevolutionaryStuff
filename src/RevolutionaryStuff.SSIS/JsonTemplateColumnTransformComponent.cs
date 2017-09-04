using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Text.RegularExpressions;
using System;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "JSON Text Template Column",
        ComponentType = ComponentType.Transform,
        IconResource = "RevolutionaryStuff.SSIS.Resources.Icon1.ico")]
    public class JsonTemplateColumnTransformComponent : BasePipelineComponent
    {
        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "JSON Text Template Column";
            ComponentMetaData.Description = "Creates a JSON column based on the given template and input rows.";

            var p = ComponentMetaData.CustomPropertyCollection.New();
            p.Name = "OutputColumnName";
            p.Description = "Name of the new derived column";

            p = ComponentMetaData.CustomPropertyCollection.New();
            p.Name = "Template";
            p.Description = "The template";

            var left = ComponentMetaData.InputCollection.New();
            left.Name = "Input";
            var matched = ComponentMetaData.OutputCollection.New();
            matched.SynchronousInputID = left.ID;
            matched.Name = "Output";
            matched.Description = "The output with the new column.";
        }

        private string OutputColumnName => ComponentMetaData.CustomPropertyCollection["OutputColumnName"].Value as string;

        private string Template => ComponentMetaData.CustomPropertyCollection["Template"].Value as string;

        public override IDTSCustomProperty100 SetComponentProperty(string propertyName, object propertyValue)
        {
            var ret = base.SetComponentProperty(propertyName, propertyValue);
            DefineOutputs();
            return ret;
        }

        public override void OnOutputPathAttached(int outputID)
        {
            base.OnOutputPathAttached(outputID);
            DefineOutputs();
        }

        private void DefineOutputs()
        {
            if (!ComponentMetaData.InputCollection[0].IsAttached) return;
            if (!ComponentMetaData.OutputCollection[0].IsAttached) return;
            if (string.IsNullOrEmpty(OutputColumnName)) return;
            DebuggerAttachmentWait();
            var input = ComponentMetaData.InputCollection[0].GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                input.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
            }
            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            outCols.RemoveAll();
//            outCols.AddOutputColumns(leftCols);
            var outCol = outCols.New();
            outCol.Name = OutputColumnName;
            outCol.SetDataTypeProperties(DataType.DT_NTEXT, 0, 0, 0, 0);
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
                        if (outCols.Count != 1)
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

        private ColumnBufferMapping InputRootBufferColumnIndicees;
        private ColumnBufferMapping OutputBufferColumnIndicees;

        public override void PreExecute()
        {
            base.PreExecute();
            InputRootBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.InputCollection[0]);
            OutputBufferColumnIndicees = GetBufferColumnIndicees(ComponentMetaData.OutputCollection[0], ComponentMetaData.InputCollection[0].Buffer);
        }

        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
            if (!ComponentMetaData.OutputCollection[0].IsAttached) return;

            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            int outputColumnPosition = OutputBufferColumnIndicees.PositionByColumnName[OutputColumnName];
            var vals = new object[JT.Fieldnames.Count];
            while (buffer.NextRow())
            {
                /*
                for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                {
                    var col = input.InputColumnCollection[z];
                    var o = GetObject(col.Name, col.DataType, z, buffer, InputRootBufferColumnIndicees);
                    OuputBuffer.SetObject(col.DataType, OutputBufferColumnIndicees.PositionByColumnName[col.Name], o);
                }
                */
                for (int z=0;z<JT.Fieldnames.Count;++z)
                {
                    var fieldName = JT.Fieldnames[z];
                    var o = GetObject(fieldName, buffer, InputRootBufferColumnIndicees);
                    o = ToJson(o);
                    vals[z] = o;
                }
                var val = JT.Format(vals);
                OuputBuffer.SetObject(DataType.DT_NTEXT, outputColumnPosition, val);
            }
        }

        private static string ToJson(object o)
        {
            if (o == null) return "null";
            if (o is bool)
            {
                return (bool)o ? "true" : "false";
            }
            else if (o is DateTime)
            {
                return "\"" + ((DateTime)o).ToRfc8601() + "\"";
            }
            var s = o.ToString();
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

        }

        PipelineBuffer OuputBuffer;
        JsonTemplate JT;

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            if (buffers.Length == 1)
            {
                OuputBuffer = buffers[0];
            }
        }

        public override void PrepareForExecute()
        {
            base.PrepareForExecute();
            JT = new JsonTemplate(Template);
        }

        private class JsonTemplate
        {
            public readonly string Template;
            public readonly string StringFormat;
            public readonly IList<string> Fieldnames;
            private static readonly Regex TemplateParseExpr = new Regex("@(\\w+)|@\\((\\w+)\\)", RegexOptions.Compiled | RegexOptions.Singleline);

            public string Format(object[] args)
                => string.Format(StringFormat, args);

            public JsonTemplate(string template)
            {
                var fieldPosByFieldName = new Dictionary<string, int>(Comparers.CaseInsensitiveStringComparer);
                Template = template = (template ?? "");
                template = template.Replace("{", "{{").Replace("}", "}}");
                StringFormat = "";
                int startAt = 0;
                Again:
                var m = TemplateParseExpr.Match(template, startAt);
                if (m.Success)
                {
                    StringFormat += template.Substring(startAt, m.Index - startAt);
                    if (m.Index > 0 && template[m.Index - 1] == '@')
                    {
                        StringFormat += m.Value.Substring(1);
                    }
                    else
                    {
                        var fieldName = StringHelpers.Coalesce(m.Groups[1].Value, m.Groups[2].Value);
                        int pos;
                        if (!fieldPosByFieldName.TryGetValue(fieldName, out pos))
                        {
                            pos = fieldPosByFieldName.Count;
                            fieldPosByFieldName[fieldName] = pos;
                        }
                        StringFormat += "{" + pos.ToString() + "}";
                    }
                    startAt = m.Index + m.Length;
                    goto Again;
                }
                else
                {
                    StringFormat += template.Substring(startAt);
                }
                Fieldnames = fieldPosByFieldName.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList().AsReadOnly();
            }
        }

    }
}
