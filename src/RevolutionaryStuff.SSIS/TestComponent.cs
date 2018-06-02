using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core;
using System;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "TestComponent2 - LeftAndOnlyLeft",
        ComponentType = ComponentType.Transform,
        NoEditor = false,
        CurrentVersion = BasePipelineComponent.AssemblyComponentVersion,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class TestComponent2 : BasePipelineComponent
    {
        protected static class PropertyNames
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
            }
        }

        public TestComponent2()
            : base(true)
        {

        }

        private IDTSInput100 LeftInput => ComponentMetaData.InputCollection[PropertyNames.InputProperties.LeftId];
        private IDTSInputColumnCollection100 LeftColumns => LeftInput.InputColumnCollection;
        private IDTSInput100 RightInput => ComponentMetaData.InputCollection[PropertyNames.InputProperties.RightId];
        private IDTSInputColumnCollection100 RightColumns => RightInput.InputColumnCollection;

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();
            base.RemoveAllInputsOutputsAndCustomProperties();

            var leftInput = ComponentMetaData.InputCollection.New();
            leftInput.Name = PropertyNames.InputProperties.LeftName;

            var rightInput = ComponentMetaData.InputCollection.New();
            rightInput.Name = PropertyNames.InputProperties.RightName;

            var output = ComponentMetaData.OutputCollection.New();
            output.ExclusionGroup = 1;
            output.SynchronousInputID = leftInput.ID;
            output.Name = "Left Join";
            output.Description = "Left rows with right columns when matched, and null right columns on a miss";
        }

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

        protected override DTSValidationStatus OnValidate()
        {
            var ret = base.OnValidate();
            Stuff.Noop(ret);
            return ret;
        }

        private void DefineOutputs()
        {
            foreach (var input in new[] {
                ComponentMetaData.InputCollection[PropertyNames.InputProperties.LeftId].GetVirtualInput(),
                ComponentMetaData.InputCollection[PropertyNames.InputProperties.RightId].GetVirtualInput()
            })
            {
                foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
                {
                    input.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
                }
            }
            SetPrimaryOutputColumnsToLeftInputColumns();
        }

        protected IDTSOutputColumnCollection100 SetPrimaryOutputColumnsToLeftInputColumns()
        {
            var pocs = ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.PrimaryOutputId].OutputColumnCollection;
            pocs.RemoveAll();
            return pocs;
        }

        private bool LeftInputProcessed, RightInputProcessed;
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

        private void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            var outputId = ComponentMetaData.OutputCollection[PropertyNames.OutputProperties.PrimaryOutputId].ID;
            while (buffer.NextRow())
            {
                buffer.DirectRow(outputId);
            }
        }

        private void ProcessRightInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            while (buffer.NextRow())
            {
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
        }

        protected virtual void OnProcessRightInputEndOfRowset()
        {
            RightInputProcessed = true;
        }
    }
}
