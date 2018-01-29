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
        private static class PropertyNames
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
        }

        protected bool IgnoreCase => GetCustomPropertyAsBool(PropertyNames.IgnoreCase);
        protected bool TrimThenNullifyEmptyStrings => GetCustomPropertyAsBool(PropertyNames.TrimThenNullifyEmptyStrings);

        protected IDTSInput100 LeftInput => ComponentMetaData.InputCollection[PropertyNames.InputProperties.LeftId];
        protected IDTSInputColumnCollection100 LeftColumns => LeftInput.InputColumnCollection;
        protected IDTSInput100 RightInput => ComponentMetaData.InputCollection[PropertyNames.InputProperties.RightId];
        protected IDTSInputColumnCollection100 RightColumns => RightInput.InputColumnCollection;


        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            var leftInput = ComponentMetaData.InputCollection.New();
            leftInput.Name = PropertyNames.InputProperties.LeftName;

            var rightInput = ComponentMetaData.InputCollection.New();
            rightInput.Name = PropertyNames.InputProperties.RightName;

            CreateCustomProperty(PropertyNames.IgnoreCase, "1", "When {1,true} the match is case insensitive, when {0,false} it is case sensitive.");
            CreateCustomProperty(PropertyNames.TrimThenNullifyEmptyStrings, "1", "When {1,true} the match should first trim and then nullify string columns, when {0,false} do not apply this transform.");

            ProvideComponentProperties(leftInput, rightInput);
        }

        protected abstract void ProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput);

        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);
            if (LeftInput.IsAttached && RightInput.IsAttached)
            {
                DefineOutputs();
            }
        }

        protected abstract void DefineOutputs();

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

        protected ColumnBufferMapping LeftInputCbm { get; private set; }

        private ColumnBufferMapping RightInputCbm;

        protected IList<string> OrderedAppendedColumnNames { get; private set; }

        protected IList<string> OrderedCommonColumnNames { get; private set; }

        public override void PreExecute()
        {
            base.PreExecute();
            LeftInputCbm = CreateColumnBufferMapping(LeftInput);
            RightInputCbm = CreateColumnBufferMapping(RightInput);

            var commonFingerprints = GetCommonInputFingerprints(false);
            var orderedAppendedColumnNames = new List<string>();
            var orderedCommonColumnNames = new List<string>();
            for (int z = 0; z < RightInput.InputColumnCollection.Count; ++z)
            {
                var col = RightInput.InputColumnCollection[z];
                var colFingerprint = col.CreateFingerprint();
                if (commonFingerprints.Contains(colFingerprint))
                {
                    orderedCommonColumnNames.Add(col.Name);
                }
                else
                {
                    orderedAppendedColumnNames.Add(col.Name);
                }
            }
            OrderedCommonColumnNames = orderedCommonColumnNames.AsReadOnly();
            OrderedAppendedColumnNames = orderedAppendedColumnNames.AsReadOnly();
        }

        private bool RightInputProcessed = false;
        private bool LeftInputProcessed = false;
        private int ComparisonFingerprintsSampled = 0;
        private int RightRowCount = 0;
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

        protected void AllDone()
        {
            LeftInputProcessed = true;
            AppendsByCommonFieldHash.Clear();
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
            var fingerprinter = new Fingerprinter(IgnoreCase, TrimThenNullifyEmptyStrings);
            var appends = new List<object>();
            while (buffer.NextRow())
            {
                for (int z = 0; z < input.InputColumnCollection.Count; ++z)
                {
                    var col = input.InputColumnCollection[z];
                    var colFingerprint = col.CreateFingerprint();
                    var o = GetObject(col.Name, buffer, RightInputCbm);
                    if (commonFingerprints.Contains(colFingerprint))
                    {
                        fingerprinter.Include(col.Name, o);
                    }
                    else
                    {
                        appends.Add(o);
                    }
                    ++RightRowCount;
                }
                var fingerprint = fingerprinter.FingerPrint;
                AppendsByCommonFieldHash.Add(fingerprint, appends.ToArray());
                fingerprinter.Clear();
                appends.Clear();
                ++rowsProcessed;
                if (ComparisonFingerprintsSampled < SampleSize)
                {
                    ++ComparisonFingerprintsSampled;
                    FireInformation(JoinerMessageCodes.ExampleFingerprint, fingerprint);
                }
            }
            FireInformation(JoinerMessageCodes.RowsProcessed, $"{rowsProcessed}");
            FireInformation(JoinerMessageCodes.AppendsByCommonFieldHash, $"{AppendsByCommonFieldHash.Count}/{AppendsByCommonFieldHash.AtomEnumerable.Count()}");
            if (buffer.EndOfRowset)
            {
                FireInformation(JoinerMessageCodes.RightColumns, $"{RightRowCount}");
                RightInputProcessed = true;
            }
        }

        private enum JoinerMessageCodes
        {
            RowsProcessed = 1,
            ExampleFingerprint = 2,
            AppendsByCommonFieldHash = 3,
            RightColumns = 7,
            CommonColumns = 5,
            LeftColumns = 6,
            DuplicateColumnFingerprint = 8,
            NoCommonColumns = 9,
        }
    }
}
