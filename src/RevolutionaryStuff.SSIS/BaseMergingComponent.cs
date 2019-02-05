using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.SSIS
{
    public abstract class BaseMergingComponent : BasePipelineComponent
    {
        protected static class MergingPropertyNames
        {
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
                public const string PrimaryOutputName = "The Output";
            }
        }

        private IDTSInput100 LeftInput => ComponentMetaData.InputCollection[MergingPropertyNames.InputProperties.LeftId];
        private IDTSInputColumnCollection100 LeftColumns => LeftInput.InputColumnCollection;
        private IDTSInput100 RightInput => ComponentMetaData.InputCollection[MergingPropertyNames.InputProperties.RightId];
        private IDTSInputColumnCollection100 RightColumns => RightInput.InputColumnCollection;
        private IDTSOutput100 PrimaryOutput => ComponentMetaData.OutputCollection[MergingPropertyNames.OutputProperties.PrimaryOutputId];
        private IDTSOutputColumnCollection100 PrimaryOutputColumns => PrimaryOutput.OutputColumnCollection;

        protected override void OnPerformUpgrade(int from, int to)
        {
            base.OnPerformUpgrade(from, to);
            if (from < 7)
            {
                RightInput.HasSideEffects = true;
            }
        }


        protected BaseMergingComponent(bool allOutputsAreSynchronous)
            : base(allOutputsAreSynchronous)
        { }

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            var leftInput = ComponentMetaData.InputCollection.New();
            leftInput.Name = MergingPropertyNames.InputProperties.LeftName;
            leftInput.HasSideEffects = true;

            var rightInput = ComponentMetaData.InputCollection.New();
            rightInput.Name = MergingPropertyNames.InputProperties.RightName;
            rightInput.HasSideEffects = true;

            var primaryOutput = ComponentMetaData.OutputCollection.New();
            primaryOutput.Name = MergingPropertyNames.OutputProperties.PrimaryOutputName;

            OnProvideComponentProperties(leftInput, rightInput, primaryOutput);
        }

        protected abstract void OnProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput, IDTSOutput100 primaryOutput);

        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);
            if (LeftInput.IsAttached && RightInput.IsAttached)
            {
                DefineOutputs();
            }
        }

        public override void ReinitializeMetaData()
        {
            base.ReinitializeMetaData();
            DefineOutputs();
        }

        private void DefineOutputs()
            => DefineOutputs(LeftColumns, RightColumns, GetCommonInputFingerprints(true));

        protected abstract void DefineOutputs(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IList<string> commonFingerprints);

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
                    FireInformation(MergingMessageCodes.LeftColumns, col.Name);
                }
                leftDefs.Increment(col.CreateFingerprint());
            }
            for (int z = 0; z < rightCols.Count; ++z)
            {
                var col = rightCols[z];
                if (fireInformationMessages)
                {
                    FireInformation(MergingMessageCodes.RightColumns, col.Name);
                }
                var fingerprint = col.CreateFingerprint();
                if (rightDefs.Contains(fingerprint))
                {
                    if (fireInformationMessages)
                    {
                        FireError(MergingMessageCodes.DuplicateColumnFingerprint, $"Column Definition [{fingerprint}] appears 2+ times in the right input");
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
                            FireError(MergingMessageCodes.DuplicateColumnFingerprint, $"Column Definition for matched [{fingerprint}] appears {kvp.Value} times in the left input");
                        }
                    }
                    else
                    {
                        commonDefs.Add(fingerprint);
                        if (fireInformationMessages)
                        {
                            FireInformation(MergingMessageCodes.CommonColumns, fingerprint);
                        }
                    }
                }
            }
            if (commonDefs.Count == 0)
            {
                if (fireInformationMessages)
                {
                    FireInformation(MergingMessageCodes.NoCommonColumns, $"There are 0 common columns between the left and right inputs");
                }
            }
            return commonDefs.ToList();
        }


        protected override DTSValidationStatus OnValidate()
        {
            var ret = base.OnValidate();
            if (ret != DTSValidationStatus.VS_ISVALID) return ret;
            if (!LeftInput.IsAttached)
            {
                FireInformation(MergingMessageCodes.ValidateError, "Validate: The left input is not attached");
                return DTSValidationStatus.VS_ISBROKEN;
            }
            if (!RightInput.IsAttached)
            {
                FireInformation(MergingMessageCodes.ValidateError, "Validate: The right input is not attached");
                return DTSValidationStatus.VS_ISBROKEN;
            }
            if (LeftInput.InputColumnCollection.Count == 0)
            {
                FireInformation(MergingMessageCodes.ValidateError, "Validate: The left input does not report any columns");
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            if (RightInput.InputColumnCollection.Count == 0)
            {
                FireInformation(MergingMessageCodes.ValidateError, "Validate: The right input does not report any columns");
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            var cfp = GetCommonInputFingerprints(false);
            return OnValidate(LeftColumns, RightColumns, PrimaryOutputColumns, cfp);
        }

        protected virtual DTSValidationStatus OnValidate(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IDTSOutputColumnCollection100 outputColumns, IList<string> commonFingerprints)
            => DTSValidationStatus.VS_ISVALID;


        private bool RightInputProcessed = false;
        private bool LeftInputProcessed = false;

        public override void PreExecute()
        {
            base.PreExecute();
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
                    case MergingPropertyNames.InputProperties.LeftId:
                        can = LeftInputProcessed || RightInputProcessed;
                        break;
                    case MergingPropertyNames.InputProperties.RightId:
                        can = !RightInputProcessed;
                        break;
                    default:
                        can = false;
                        break;
                }
                canProcess[i] = can;
            }
        }

        protected override void OnProcessInput(int inputId, PipelineBuffer buffer)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputId);
            switch (input.Name)
            {
                case MergingPropertyNames.InputProperties.LeftName:
                    if (!LeftInputProcessed)
                    {
                        ProcessLeftInput(input, buffer);
                    }
                    break;
                case MergingPropertyNames.InputProperties.RightName:
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

        protected abstract void ProcessRightInput(IDTSInput100 input, PipelineBuffer buffer);

        protected override void OnProcessInputEndOfRowset(int inputID)
        {
            base.OnProcessInputEndOfRowset(inputID);
            int inputIndex = ComponentMetaData.InputCollection.GetObjectIndexByID(inputID);
            if (inputIndex == MergingPropertyNames.InputProperties.LeftId)
            {
                LeftInputProcessed = true;
                OnProcessLeftInputEndOfRowset();
            }
            else if (inputIndex == MergingPropertyNames.InputProperties.RightId)
            {
                RightInputProcessed = true;
                OnProcessRightInputEndOfRowset();
            }
        }

        protected virtual void OnProcessLeftInputEndOfRowset()
        { }

        protected virtual void OnProcessRightInputEndOfRowset()
        { }

        protected enum MergingMessageCodes
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

        protected class MergingRuntimeData : RuntimeData
        {
            protected new BaseMergingComponent Parent
                => (BaseMergingComponent)base.Parent;

            public readonly ColumnBufferMapping LeftInputCbm;
            public readonly ColumnBufferMapping RightInputCbm;
            public readonly ColumnBufferMapping PrimaryOutputCbm;
            public readonly IDTSOutput100 PrimaryOutput;
            public readonly bool PrimaryOutputIsAttached;

            public MergingRuntimeData(BaseMergingComponent parent)
                : base(parent)
            {
                LeftInputCbm = InputColumnBufferMappings[0];
                RightInputCbm = InputColumnBufferMappings[1];
                PrimaryOutput = Parent.PrimaryOutput;
                PrimaryOutputCbm = OutputColumnBufferMappings[0];
                PrimaryOutputIsAttached = PrimaryOutput.IsAttached;
            }
        }

        protected override RuntimeData ConstructRuntimeData()
            => new MergingRuntimeData(this);

        private new MergingRuntimeData RD
            => (MergingRuntimeData)base.RD;
    }
}
