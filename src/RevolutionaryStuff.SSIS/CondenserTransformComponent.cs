using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Crypto;
using System;
using System.Text;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "Condenser",
        ComponentType = ComponentType.Transform,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class CondenserTransformComponent : BasePipelineComponent
    {
        private static class PropertyNames
        {
            public const string OutputColumnName = CommonPropertyNames.OutputColumnName;
            public const string OutputColumnLength = "OutputColumnLength";
            public const string OutputColumnCodePage = "OutputColumnCodePage";
            public const string InputColumnName = "ValueFieldName";
            public const string PreservedPrefix = "PreservedPrefix";
            public const string PreservedSuffix = "PreservedSuffix";
            public const string AlwaysTransform = "AlwaysTransform";
            public const string TransformAlgorithm = "TransformAlgorithm";

            public static class InputProperties
            {
                public const int TheInputId = 0;
                public const string TheInputName = "The Input";
            }

            public static class OutputProperties
            {
                public const int TheOutputId = 0;
                public const string TheOutputName = "The Output";
            }
        }

        private enum TransformAlgorithms
        {
            MidlineEllipses,
            CRC32,
            MD5,
            SHA1,
        }

        private TransformAlgorithms TransformAlgorithm
            => Parse.ParseEnum<TransformAlgorithms>(base.GetCustomPropertyAsString(PropertyNames.TransformAlgorithm), TransformAlgorithms.MidlineEllipses);

        private bool AlwaysTransform
            => base.GetCustomPropertyAsBool(PropertyNames.AlwaysTransform, true);

        private bool OutputColumnIsUnicode
            => OutputColumnCodePage == 0;

        private int OutputColumnCodePage
            => base.GetCustomPropertyAsInt(PropertyNames.OutputColumnCodePage, 0);

        private int OutputColumnLength
            => base.GetCustomPropertyAsInt(PropertyNames.OutputColumnLength, 50);

        private string OutputColumnName
            => base.GetCustomPropertyAsString(PropertyNames.OutputColumnName);

        private string InputColumnName
            => base.GetCustomPropertyAsString(PropertyNames.InputColumnName).Trim().ToLower();

        private string PreservedPrefix
            => base.GetCustomPropertyAsString(PropertyNames.PreservedPrefix) ?? "";

        private string PreservedSuffix
            => base.GetCustomPropertyAsString(PropertyNames.PreservedSuffix) ?? "";

        IDTSInput100 TheInput => ComponentMetaData.InputCollection[PropertyNames.InputProperties.TheInputId];
        IDTSInputColumnCollection100 TheInputColumns => TheInput.InputColumnCollection;
        IDTSOutput100 TheOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.TheOutputId];
        IDTSOutputColumnCollection100 TheOutputColumns => TheOutput.OutputColumnCollection;

        public CondenserTransformComponent()
            : base()
        { }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "Condenser";
            ComponentMetaData.Description = "Brings a columns value down to size";

            CreateCustomProperty(PropertyNames.OutputColumnName, "CondensedField", "The name of the field where the condensed value will be stored.");
            CreateCustomProperty(PropertyNames.OutputColumnLength, "50", "The length of the field where the condensed value will be stored.");
            CreateCustomProperty(PropertyNames.OutputColumnCodePage, "0", "When {0}, the output column is unicode, otherwise it is a string with the given code page.");
            CreateCustomProperty(PropertyNames.InputColumnName, "InputField", "The name of the input field");
            CreateCustomProperty(PropertyNames.PreservedPrefix, "", "A prefix that will always be present regardless of the condensing");
            CreateCustomProperty(PropertyNames.PreservedSuffix, "", "A suffix that will always be present regardless of the condensing");
            CreateCustomProperty(PropertyNames.AlwaysTransform, "1", "When {1,true} the input value will always be transformed;  When {0,false} only values resulting in an excessive output length will be transformed");
            CreateCustomProperty(PropertyNames.TransformAlgorithm, $"{TransformAlgorithms.MidlineEllipses}", $"The name of the transformation algorithm to use {Enum.GetNames(typeof(TransformAlgorithms)).Format("|")}");

            var left = ComponentMetaData.InputCollection.New();
            left.Name = PropertyNames.InputProperties.TheInputName;
            var matched = ComponentMetaData.OutputCollection.New();
            matched.SynchronousInputID = left.ID;
            matched.Name = PropertyNames.OutputProperties.TheOutputName;
            matched.Description = "The input row with the condensed column.";
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

        public override void ReinitializeMetaData()
        {
            base.ReinitializeMetaData();
            DefineOutputs();
        }

        private void DefineOutputs()
        {
            if (!ComponentMetaData.InputCollection[0].IsAttached) return;
            if (!ComponentMetaData.OutputCollection[0].IsAttached) return;
            if (string.IsNullOrEmpty(OutputColumnName)) return;
            DebuggerAttachmentWait();
            var input = ComponentMetaData.InputCollection[0].GetVirtualInput();
            var incolname = InputColumnName;
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                input.SetUsageType(vcol.LineageID, SsisHelpers.IsColumnNameMatch(incolname, vcol.Name) ? DTSUsageType.UT_READONLY : DTSUsageType.UT_IGNORED);
            }
            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            outCols.RemoveAll();
            var outCol = outCols.New();
            outCol.Name = OutputColumnName;
            if (OutputColumnIsUnicode)
            {
                outCol.SetDataTypeProperties(DataType.DT_WSTR, OutputColumnLength, 0, 0, 0);
            }
            else
            {
                outCol.SetDataTypeProperties(DataType.DT_STR, OutputColumnLength, 0, 0, OutputColumnCodePage);
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
                        if (outCols.Count != 1)
                        {
                            ret = DTSValidationStatus.VS_ISBROKEN;
                        }
                    }
                    break;
            }
            return ret;
        }

        private ColumnBufferMapping InputRootBufferColumnIndicees;
        private ColumnBufferMapping OutputBufferColumnIndicees;

        public override void PreExecute()
        {
            base.PreExecute();
            InputRootBufferColumnIndicees = CreateColumnBufferMapping(ComponentMetaData.InputCollection[0]);
            OutputBufferColumnIndicees = CreateColumnBufferMapping(ComponentMetaData.OutputCollection[0], ComponentMetaData.InputCollection[0].Buffer);
        }

        PipelineBuffer OuputBuffer;

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            if (buffers.Length == 1)
            {
                OuputBuffer = buffers[0];
            }
        }
        protected override void OnProcessInput(int inputID, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
            if (!ComponentMetaData.OutputCollection[0].IsAttached) return;

            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            int inputColumnPosition = InputRootBufferColumnIndicees.GetPositionFromColumnName(InputColumnName);
            int outputColumnPosition = OutputBufferColumnIndicees.GetPositionFromColumnName(OutputColumnName);
            int maxValLen = OutputColumnLength - PreservedPrefix.Length - PreservedSuffix.Length;
            var prefix = PreservedPrefix;
            var suffix = PreservedSuffix;
            bool always = AlwaysTransform;
            var alg = TransformAlgorithm;
            while (buffer.NextRow())
            {
                var val = buffer[inputColumnPosition] as string ?? "";
                if (always || val.Length > maxValLen)
                {
                    switch (alg)
                    {
                        case TransformAlgorithms.MidlineEllipses:
                            val = StringHelpers.TruncateWithMidlineEllipsis(val, maxValLen);
                            break;
                        case TransformAlgorithms.CRC32:
                            {
                                var buf = Encoding.UTF8.GetBytes(val);
                                val = CRC32Checksum.Do(buf).ToString();
                            }
                            break;
                        case TransformAlgorithms.MD5:
                            {
                                var buf = Encoding.UTF8.GetBytes(val);
                                val = Hash.Compute(buf, Hash.CommonHashAlgorithmNames.Md5).DataHuman;
                            }
                            break;
                        case TransformAlgorithms.SHA1:
                            {
                                var buf = Encoding.UTF8.GetBytes(val);
                                val = Hash.Compute(buf, Hash.CommonHashAlgorithmNames.Sha1).DataHuman;
                            }
                            break;
                        default:
                            throw new UnexpectedSwitchValueException(alg);
                    }
                }
                val = $"{PreservedPrefix}{val}{PreservedSuffix}";
                buffer[outputColumnPosition] = val;
            }
        }
    }
}
