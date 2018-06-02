using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core.Collections;
using RevolutionaryStuff.Core;


namespace RevolutionaryStuff.SSIS
{
    public abstract class BaseJoinerComponent : BasePipelineComponent
    {
        protected static class PropertyNames
        {
            public const string IgnoreCase = CommonPropertyNames.IgnoreCase;
            public const string TrimThenNullifyEmptyStrings = "TrimThenNullifyEmptyStrings";

            public static class InputProperties
            {
                public const int LeftId = 0;
                public const string LeftName = "Left Input";
                public const int RightId = 1;
                public const string RightName = "Right Input";
            }
            public static class OutputProperties
            {
                public const int PrimaryOutputId = 0;
            }
        }

        protected BaseJoinerComponent(bool allOutputsAreSynchronous)
            : base(allOutputsAreSynchronous)
        { }

        protected class JoinerRuntimeData : RuntimeData
        {
            protected new BaseJoinerComponent Parent 
                => (BaseJoinerComponent)base.Parent;

            public readonly IList<string> CommonFingerprints;
            public readonly ColumnBufferMapping LeftInputCbm;
            public readonly ColumnBufferMapping RightInputCbm;
            public readonly ColumnBufferMapping PrimaryOutputCbm;
            public readonly IDTSOutput100 PrimaryOutput;
            public readonly bool PrimaryOutputIsAttached;
            public readonly int PrimaryOutputId;

            public readonly bool IgnoreCase;
            public readonly bool TrimThenNullifyEmptyStrings;
            public int ComparisonFingerprintsSampled = 0;
            public int RightRowCount = 0;

            public readonly IList<string> OrderedAppendedColumnNames;
            public readonly IList<int> OrderedAppendedPrimaryOutputColumnIndicees;
            public readonly IList<string> OrderedCommonColumnNames;

            internal Fingerprinter CreateFingerprinter()
                => new Fingerprinter(IgnoreCase, TrimThenNullifyEmptyStrings);

            protected JoinerRuntimeData(BaseJoinerComponent parent, bool appendSomeRightColumns)
                : base(parent)
            {
                IgnoreCase = GetCustomPropertyAsBool(PropertyNames.IgnoreCase);
                TrimThenNullifyEmptyStrings = GetCustomPropertyAsBool(PropertyNames.TrimThenNullifyEmptyStrings);
                LeftInputCbm = InputColumnBufferMappings[0];
                RightInputCbm = InputColumnBufferMappings[1];
                PrimaryOutput = ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.PrimaryOutputId];
                PrimaryOutputCbm = OutputColumnBufferMappings[0];
                PrimaryOutputIsAttached = PrimaryOutput.IsAttached;
                PrimaryOutputId = PrimaryOutput.ID;
                CommonFingerprints = Parent.GetCommonInputFingerprints(false).AsReadOnly();

                var orderedAppendedColumnNames = new List<string>();
                var orderedCommonColumnNames = new List<string>();
                var orderedAppendedColumnIndicees = new List<int>();
                var rightInput = ComponentMetaData.InputCollection[1];
                for (int z = 0; z < rightInput.InputColumnCollection.Count; ++z)
                {
                    var col = rightInput.InputColumnCollection[z];
                    var colFingerprint = col.CreateFingerprint();
                    if (CommonFingerprints.Contains(colFingerprint))
                    {
                        orderedCommonColumnNames.Add(col.Name);
                    }
                    else if (appendSomeRightColumns)
                    {
                        orderedAppendedColumnNames.Add(col.Name);
                        orderedAppendedColumnIndicees.Add(PrimaryOutputCbm.GetPositionFromColumnName(col.Name));
                    }
                }
                OrderedCommonColumnNames = orderedCommonColumnNames.AsReadOnly();
                OrderedAppendedColumnNames = orderedAppendedColumnNames.AsReadOnly();
                OrderedAppendedPrimaryOutputColumnIndicees = orderedAppendedColumnIndicees.AsReadOnly();
            }
        }

        private new JoinerRuntimeData RD
            => (JoinerRuntimeData)base.RD;

        private IDTSInput100 LeftInput => ComponentMetaData.InputCollection[PropertyNames.InputProperties.LeftId];
        private IDTSInputColumnCollection100 LeftColumns => LeftInput.InputColumnCollection;
        private IDTSInput100 RightInput => ComponentMetaData.InputCollection[PropertyNames.InputProperties.RightId];
        private IDTSInputColumnCollection100 RightColumns => RightInput.InputColumnCollection;
        private IDTSOutput100 PrimaryOutput => ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.PrimaryOutputId];
        private IDTSOutputColumnCollection100 PrimaryOutputColumns => PrimaryOutput.OutputColumnCollection;

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            var leftInput = ComponentMetaData.InputCollection.New();
            leftInput.Name = PropertyNames.InputProperties.LeftName;

            var rightInput = ComponentMetaData.InputCollection.New();
            rightInput.Name = PropertyNames.InputProperties.RightName;
            rightInput.HasSideEffects = true;

            CreateCustomProperty(PropertyNames.IgnoreCase, "1", "When {1,true} the match is case insensitive, when {0,false} it is case sensitive.");
            CreateCustomProperty(PropertyNames.TrimThenNullifyEmptyStrings, "1", "When {1,true} the match should first trim and then nullify string columns, when {0,false} do not apply this transform.");

            OnProvideComponentProperties(leftInput, rightInput);
        }

        protected override void OnPerformUpgrade(int from, int to)
        {
            base.OnPerformUpgrade(from, to);
            if (from < 4)
            {
                RightInput.HasSideEffects = true;
            }
        }

        protected abstract void OnProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput);

        protected override DTSValidationStatus OnValidate()
        {
            var ret = base.OnValidate();
            if (ret != DTSValidationStatus.VS_ISVALID) return ret;
            if (!LeftInput.IsAttached)
            {
                FireInformation(JoinerMessageCodes.ValidateError, "Validate: The left input is not attached");
                return DTSValidationStatus.VS_ISBROKEN;
            }
            if (!RightInput.IsAttached)
            {
                FireInformation(JoinerMessageCodes.ValidateError, "Validate: The right input is not attached");
                return DTSValidationStatus.VS_ISBROKEN;
            }
            if (LeftInput.InputColumnCollection.Count == 0)
            {
                FireInformation(JoinerMessageCodes.ValidateError, "Validate: The left input does not report any columns");
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            if (RightInput.InputColumnCollection.Count == 0)
            {
                FireInformation(JoinerMessageCodes.ValidateError, "Validate: The right input does not report any columns");
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            var cfp = GetCommonInputFingerprints(false);
            if (cfp.Count == 0)
            {
                FireInformation(JoinerMessageCodes.ValidateError, "Validate: There are no commom fingerprints");
                GetCommonInputFingerprints(true); // to show the issue...
                return DTSValidationStatus.VS_ISBROKEN;
            }
            /*
            if (PrimaryOutputColumns.Count < LeftColumns.Count)
            {
                FireInformation(JoinerMessageCodes.ValidateError, "Validate: There are fewer output columns than left input columns");
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            if (PrimaryOutputColumns.Count > LeftColumns.Count + RightColumns.Count)
            {
                FireInformation(JoinerMessageCodes.ValidateError, "Validate: There are more output columns than left + right input columns");
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            */
            return OnValidate(LeftColumns, RightColumns, PrimaryOutputColumns, cfp);
        }

        protected virtual DTSValidationStatus OnValidate(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IDTSOutputColumnCollection100 outputColumns, IList<string> commonFingerprints)
        {
            return DTSValidationStatus.VS_ISVALID;
        }

        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);
            if (LeftInput.IsAttached && RightInput.IsAttached)
            {
                DefineOutputs();
            }
        }

        private void DefineOutputs()
        {
            DefineOutputs(LeftColumns, RightColumns, GetCommonInputFingerprints(true));
        }

        protected IDTSOutputColumnCollection100 SetPrimaryOutputColumnsToLeftInputColumns()
        {
            var pocs = ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.PrimaryOutputId].OutputColumnCollection;
            pocs.RemoveAll();
            return pocs;
        }

        protected abstract void DefineOutputs(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IList<string> commonFingerprints);

        public override void ReinitializeMetaData()
        {
            base.ReinitializeMetaData();
            DefineOutputs();
        }

        protected virtual void SetInputColumnUsage(DTSUsageType leftOnlyColumnUsage=DTSUsageType.UT_IGNORED)
        {
            var commonFingerprints = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            foreach (var fingerprint in GetCommonInputFingerprints(true))
            {
                commonFingerprints.Add(fingerprint);
            }
            var input = ComponentMetaData.InputCollection[PropertyNames.InputProperties.LeftId].GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                var usage = commonFingerprints.Contains(vcol.CreateFingerprint()) ? DTSUsageType.UT_READONLY : leftOnlyColumnUsage;
                input.SetUsageType(vcol.LineageID, usage);
            }
            input = ComponentMetaData.InputCollection[PropertyNames.InputProperties.RightId].GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                input.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
            }
        }

        private bool RightInputProcessed = false;
        private bool LeftInputProcessed = false;
        protected MultipleValueDictionary<string, object[]> AppendsByCommonFieldHash { get; private set; }

        public override void PrepareForExecute()
        {
            base.PrepareForExecute();
            AppendsByCommonFieldHash = new MultipleValueDictionary<string, object[]>();
            RightInputProcessed = false;
            LeftInputProcessed = false;
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

        protected override void OnProcessInputEndOfRowset(int inputID)
        {
            base.OnProcessInputEndOfRowset(inputID);
            int inputIndex = ComponentMetaData.InputCollection.GetObjectIndexByID(inputID);
            if (inputIndex == PropertyNames.InputProperties.LeftId)
            {
                OnProcessLeftInputEndOfRowset();
            }
            else if (inputIndex == PropertyNames.InputProperties.RightId)
            {
                OnProcessRightInputEndOfRowset();
            }
        }

        protected virtual void OnProcessLeftInputEndOfRowset()
        {
            LeftInputProcessed = true;
            AppendsByCommonFieldHash.Clear();
        }

        protected virtual void OnProcessRightInputEndOfRowset()
        {
            RightInputProcessed = true;
        }

        protected IList<string> GetCommonInputFingerprints(bool fireInformationMessages = false)
        {
            var leftCols = LeftInput.GetVirtualInput().VirtualInputColumnCollection;
            var rightCols = RightInput.GetVirtualInput().VirtualInputColumnCollection;
            var leftDefs = new Dictionary<string, int>(Comparers.CaseInsensitiveStringComparer);
            var rightDefs = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            for (int z = 0; z < leftCols.Count; ++z)
            {
                var col = leftCols[z];
                if (fireInformationMessages)
                {
                    FireInformation(JoinerMessageCodes.LeftColumns, col.Name);
                }
                leftDefs.Increment(col.CreateFingerprint());
            }
            for (int z = 0; z < rightCols.Count; ++z)
            {
                var col = rightCols[z];
                if (fireInformationMessages)
                {
                    FireInformation(JoinerMessageCodes.RightColumns, col.Name);
                }
                var fingerprint = col.CreateFingerprint();
                if (rightDefs.Contains(fingerprint))
                {
                    if (fireInformationMessages)
                    {
                        FireError(JoinerMessageCodes.DuplicateColumnFingerprint, $"Column Definition [{fingerprint}] appears 2+ times in the right input");
                    }
                }
                else
                {
                    rightDefs.Add(fingerprint);
                }
            }
            var commonDefs = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            foreach (var kvp in leftDefs)
            {
                var fingerprint = kvp.Key;
                if (rightDefs.Contains(fingerprint))
                {
                    if (kvp.Value > 1)
                    {
                        if (fireInformationMessages)
                        {
                            FireError(JoinerMessageCodes.DuplicateColumnFingerprint, $"Column Definition for matched [{fingerprint}] appears {kvp.Value} times in the left input");
                        }
                    }
                    else
                    {
                        commonDefs.Add(fingerprint);
                        if (fireInformationMessages)
                        {
                            FireInformation(JoinerMessageCodes.CommonColumns, fingerprint);
                        }
                    }
                }
            }
            if (commonDefs.Count == 0)
            {
                if (fireInformationMessages)
                {
                    FireError(JoinerMessageCodes.NoCommonColumns, $"There are 0 common columns between the left and right inputs");
                }
            }
            return commonDefs.ToList();
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

        protected abstract void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer);

        private void ProcessRightInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            int rowsProcessed = 0;
            var commonFingerprints = GetCommonInputFingerprints();
            var fingerprinter = new Fingerprinter(RD.IgnoreCase, RD.TrimThenNullifyEmptyStrings);
            var appends = new List<object>();
            while (buffer.NextRow())
            {
                for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                {
                    var col = input.InputColumnCollection[z];
                    var colFingerprint = col.CreateFingerprint();
                    var o = GetObject(col.Name, buffer, RD.RightInputCbm);
                    if (commonFingerprints.Contains(colFingerprint))
                    {
                        fingerprinter.Include(col.Name, o);
                    }
                    else
                    {
                        appends.Add(o);
                    }
                    ++RD.RightRowCount;
                }
                var fingerprint = fingerprinter.FingerPrint;
                AppendsByCommonFieldHash.Add(fingerprint, appends.ToArray());
                fingerprinter.Clear();
                appends.Clear();
                ++rowsProcessed;
                if (RD.ComparisonFingerprintsSampled < SampleSize)
                {
                    ++RD.ComparisonFingerprintsSampled;
                    FireInformation(JoinerMessageCodes.ExampleFingerprint, fingerprint);
                }
            }
            FireInformation(JoinerMessageCodes.RowsProcessed, $"{rowsProcessed}");
            FireInformation(JoinerMessageCodes.AppendsByCommonFieldHash, $"{AppendsByCommonFieldHash.Count}/{AppendsByCommonFieldHash.AtomEnumerable.Count()}");
        }

        protected enum JoinerMessageCodes
        {
            RowsProcessed = 1,
            ExampleFingerprint = 2,
            AppendsByCommonFieldHash = 3,
            RightColumns = 7,
            CommonColumns = 5,
            LeftColumns = 6,
            DuplicateColumnFingerprint = 8,
            NoCommonColumns = 9,
            ValidateError = 10,
        }
    }
}
