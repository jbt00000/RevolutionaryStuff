﻿using System.Collections.Generic;
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
        NoEditor = false,
        CurrentVersion = BasePipelineComponent.AssemblyComponentVersion,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class JsonTemplateColumnTransformComponent : BasePipelineComponent
    {
        private static class PropertyNames
        {
            public const string OutputColumnName = CommonPropertyNames.OutputColumnName;
            public const string Template = "Template";
        }

        public JsonTemplateColumnTransformComponent()
            : base(true)
        { }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "JSON Text Template Column";
            ComponentMetaData.Description = "Creates a JSON column based on the given template and input rows.";

            CreateCustomProperty(PropertyNames.OutputColumnName, "Json", "Name of the new derived column");
            CreateCustomProperty(PropertyNames.Template, null, "The template.  use @ColumnName or @(Column Name)");

            var left = ComponentMetaData.InputCollection.New();
            left.Name = "Input";
            var matched = ComponentMetaData.OutputCollection.New();
            matched.SynchronousInputID = left.ID;
            matched.Name = "Output";
            matched.Description = "The output with the new column.";
        }

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
            var tf = new HashSet<string>(new JsonTemplate(Template).Fieldnames, Comparers.CaseInsensitiveStringComparer);
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                input.SetUsageType(vcol.LineageID, tf.Contains(vcol.Name) ? DTSUsageType.UT_READONLY : DTSUsageType.UT_IGNORED);
            }
            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            outCols.RemoveAll();
//            outCols.AddOutputColumns(leftCols);
            var outCol = outCols.New();
            outCol.Name = OutputColumnName;
            outCol.SetDataTypeProperties(DataType.DT_NTEXT, 0, 0, 0, 0);
        }

        protected override DTSValidationStatus OnValidate()
        {
            var ret = base.OnValidate();
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

        private string OutputColumnName 
            => GetCustomPropertyAsString(PropertyNames.OutputColumnName);

        private string Template 
            => GetCustomPropertyAsString(PropertyNames.Template);

        private enum InformationMessageCodes
        {
            TemplateFieldMissing = 1,
            FieldMapInformation = 2,
        }

        public override void PreExecute()
        {
            base.PreExecute();
            InputRootBufferColumnIndicees = CreateColumnBufferMapping(ComponentMetaData.InputCollection[0]);
            OutputBufferColumnIndicees = CreateColumnBufferMapping(ComponentMetaData.OutputCollection[0], ComponentMetaData.InputCollection[0].Buffer);
            int missingCount = 0;
            foreach (var fieldName in JT.Fieldnames)
            {
                if (!InputRootBufferColumnIndicees.ColumnExists(fieldName))
                {
                    FireInformation(InformationMessageCodes.TemplateFieldMissing, $"[{fieldName}] cannot be found. Processing will be {Stuff.Qbert}");
                    ++missingCount;
                }
            }
            FireInformation(InformationMessageCodes.FieldMapInformation, $"There are {JT.Fieldnames.Count}. {missingCount} are missing");
        }

        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
            if (!ComponentMetaData.OutputCollection[0].IsAttached) return;

            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            int outputColumnPosition = OutputBufferColumnIndicees.GetPositionFromColumnName(OutputColumnName);
            var vals = new object[JT.Fieldnames.Count];
            while (buffer.NextRow())
            {
                for (int z=0;z<JT.Fieldnames.Count;++z)
                {
                    var fieldName = JT.Fieldnames[z];
                    var o = GetObject(fieldName, buffer, InputRootBufferColumnIndicees);
                    o = ToJson(o);
                    vals[z] = o;
                }
                var val = JT.Format(vals);
                buffer.SetObject(DataType.DT_NTEXT, outputColumnPosition, val);
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
            else if (o is int)
            {
                return o.ToString();
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
