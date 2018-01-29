using System;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using IDTSVirtualInputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInputColumn100;
using IDTSVirtualInput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInput100;

namespace RevolutionaryStuff.SSIS
{
    /// <remarks>https://github.com/keif888/NullDestination</remarks>
    [DtsPipelineComponent(
           DisplayName = "Null Destination",
           Description = "Destination that just dumps all the records into a null output.",
           IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico",
           ComponentType = ComponentType.DestinationAdapter,
           CurrentVersion = 0)]
    public class NullDestination : PipelineComponent
    {
        /// <summary>
        /// stores whether there are issues with input columns as a result from Validate.
        /// </summary>
        private bool areInputColumnsValid = true;

        /// <summary>
        /// Sets the basic infirmation about the component.
        /// </summary>
        public override void ProvideComponentProperties()
        {
            // Remove anything that shouldn't be here.
            this.RemoveAllInputsOutputsAndCustomProperties();
            // Remove any connection collections.
            ComponentMetaData.RuntimeConnectionCollection.RemoveAll();
            // Create an input
            ComponentMetaData.InputCollection.New();
            // Assign a name.
            ComponentMetaData.InputCollection[0].Name = "TrashInput";
            ComponentMetaData.InputCollection[0].HasSideEffects = true;
            // Set the contact information.
            ComponentMetaData.ContactInfo = "https://github.com/keif888/NullDestination/";
        }

        /// <summary>
        /// Upgrade the metadata if it needs it.
        /// Right now all this does is update the version number in the XML.
        /// </summary>
        /// <param name="pipelineVersion">The curreht version of the pipeline.</param>
        public override void PerformUpgrade(int pipelineVersion)
        {
            // Get the attributes for the executable
            DtsPipelineComponentAttribute componentAttribute = (DtsPipelineComponentAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(DtsPipelineComponentAttribute), false);
            int binaryVersion = componentAttribute.CurrentVersion;

            // Set the SSIS Package's version ID for this component to the binary version...
            ComponentMetaData.Version = binaryVersion;
        }

        /// <summary>
        /// Called repeatedly when the component is edited in the designer, and once at the beginning of execution.
        /// Verifies the following:
        /// 1. Check that there are no outputs
        /// 2. Check that there is only one input
        /// 3. Check that all upstream columns are present.
        /// </summary>
        /// <returns>The status of the validation</returns>
        public override DTSValidationStatus Validate()
        {
            bool cancel = false;
            if (ComponentMetaData.InputCollection.Count != 1)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "The input collection count is not 1.", String.Empty, 0, out cancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            /*

            if (ComponentMetaData.OutputCollection.Count != 0)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "The output collection count is not 0.", String.Empty, 0, out cancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            IDTSInput input = ComponentMetaData.InputCollection[0];
            IDTSVirtualInput vInput = input.GetVirtualInput();

            if (input.HasSideEffects == false)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "The input does not have HasSideEffects set.", String.Empty, 0, out cancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            foreach (IDTSInputColumn inputColumn in input.InputColumnCollection)
            {
                try
                {
                    IDTSVirtualInputColumn vColumn = vInput.VirtualInputColumnCollection.GetVirtualInputColumnByLineageID(inputColumn.LineageID);
                }
                catch
                {
                    ComponentMetaData.FireError(0, ComponentMetaData.Name, "The input column " + inputColumn.IdentificationString + " does not match a column in the upstream output.", String.Empty, 0, out cancel);
                    areInputColumnsValid = false;
                    return DTSValidationStatus.VS_NEEDSNEWMETADATA;
                }
            }
            //return base.Validate();
            */
            return DTSValidationStatus.VS_ISVALID;
        }

        /// <summary>
        /// If there are validation issues, then repair them!
        /// The only issue that can be repaired is when there are missing upstream columns.
        /// </summary>
        public override void ReinitializeMetaData()
        {
            if (!areInputColumnsValid)
            {
                int inputID = ComponentMetaData.InputCollection[0].ID;
                // Remove all the current columns.
                ComponentMetaData.InputCollection[0].InputColumnCollection.RemoveAll();
                // Loop though all the columns in the input path, and connect them.
                IDTSVirtualInput virtualInput = ComponentMetaData.InputCollection[0].GetVirtualInput();
                if (virtualInput == null)
                {
                    throw new ArgumentNullException("virtualInput");
                }

                foreach (IDTSVirtualInputColumn viColumn in virtualInput.VirtualInputColumnCollection)
                {
                    this.SetUsageType(inputID, virtualInput, viColumn.LineageID, DTSUsageType.UT_READONLY);
                }
                areInputColumnsValid = true;
            }
            base.ReinitializeMetaData();
        }

        /// <summary>
        /// If a path is connected, automatically select all the columns.
        /// </summary>
        /// <param name="inputID">The internal id of the input.  Should always translate to index 0, but...</param>
        public override void OnInputPathAttached(int inputID)
        {
            // Get the index of the input in the collection
            int inputIndex = ComponentMetaData.InputCollection.FindObjectIndexByID(inputID);
            // Remove all the current columns.
            ComponentMetaData.InputCollection[inputIndex].InputColumnCollection.RemoveAll();
            // Loop though all the columns in the input path, and connect them.
            IDTSVirtualInput virtualInput = ComponentMetaData.InputCollection[inputIndex].GetVirtualInput();
            if (virtualInput == null)
            {
                throw new ArgumentNullException("virtualInput");
            }

            foreach (IDTSVirtualInputColumn viColumn in virtualInput.VirtualInputColumnCollection)
            {
                this.SetUsageType(inputID, virtualInput, viColumn.LineageID, DTSUsageType.UT_READONLY);
            }
        }

        /// <summary>
        /// When an input is detached, remove all the columns in the input's collection.
        /// </summary>
        /// <param name="inputID">The internal id of the input.  Should always translate to index 0, but...</param>
        public override void OnInputPathDetached(int inputID)
        {
            int inputIndex = ComponentMetaData.InputCollection.FindObjectIndexByID(inputID);
            ComponentMetaData.InputCollection[inputIndex].InputColumnCollection.RemoveAll();
        }

        /// <summary>
        /// For every record received, do nothing.
        /// </summary>
        /// <param name="inputID">The internal id of the input.  Should always translate to index 0, but...</param>
        /// <param name="buffer">The SSIS buffer that contains the data that is supposed to be processed.</param>
        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            base.ProcessInput(inputID, buffer);
            while (buffer.NextRow())
            {
                // Do Nothing
            }
        }
    }
}
