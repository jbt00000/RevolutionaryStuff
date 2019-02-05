using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core.Collections;
using RevolutionaryStuff.Core;


namespace RevolutionaryStuff.SSIS
{
    public abstract class BaseJoinerComponent : BaseMergingComponent
    {
        protected static class PropertyNames
        {
            public const string IgnoreCase = CommonPropertyNames.IgnoreCase;
            public const string TrimThenNullifyEmptyStrings = "TrimThenNullifyEmptyStrings";
        }

        protected BaseJoinerComponent(bool allOutputsAreSynchronous)
            : base(allOutputsAreSynchronous)
        { }

        protected override DTSValidationStatus OnValidate(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IDTSOutputColumnCollection100 outputColumns, IList<string> commonFingerprints)
        {
            var ret = base.OnValidate(leftColumns, rightColumns, outputColumns, commonFingerprints);
            if (ret != DTSValidationStatus.VS_ISVALID) return ret;
            if (commonFingerprints.Count == 0)
            {
                FireInformation(JoinerMessageCodes.NoCommonColumns, "Validate: There are no commom fingerprints");
                GetCommonInputFingerprints(true); // to show the issue...
                return DTSValidationStatus.VS_ISBROKEN;
            }
            return OnValidatedEnsureSingleOutput();
        }

        protected virtual DTSValidationStatus OnValidatedEnsureSingleOutput()
            => ComponentMetaData.OutputCollection.Count == 1 ? DTSValidationStatus.VS_ISVALID : DTSValidationStatus.VS_ISBROKEN;

        protected class JoinerRuntimeData : MergingRuntimeData
        {
            protected new BaseJoinerComponent Parent
                => (BaseJoinerComponent)base.Parent;

            public readonly IList<string> CommonFingerprints;
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

        protected override void OnProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput, IDTSOutput100 primaryOutput)
        {
            CreateCustomProperty(PropertyNames.IgnoreCase, "1", "When {1,true} the match is case insensitive, when {0,false} it is case sensitive.");
            CreateCustomProperty(PropertyNames.TrimThenNullifyEmptyStrings, "1", "When {1,true} the match should first trim and then nullify string columns, when {0,false} do not apply this transform.");
            primaryOutput.ExclusionGroup = 1;
            primaryOutput.SynchronousInputID = leftInput.ID;
        }

        protected IDTSOutputColumnCollection100 SetPrimaryOutputColumnsToLeftInputColumns()
        {
            var pocs = ComponentMetaData.OutputCollection[MergingPropertyNames.OutputProperties.PrimaryOutputId].OutputColumnCollection;
            pocs.RemoveAll();
            return pocs;
        }

        protected virtual void SetInputColumnUsage(DTSUsageType leftOnlyColumnUsage=DTSUsageType.UT_IGNORED)
        {
            var commonFingerprints = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            foreach (var fingerprint in GetCommonInputFingerprints(true))
            {
                commonFingerprints.Add(fingerprint);
            }
            var input = ComponentMetaData.InputCollection[MergingPropertyNames.InputProperties.LeftId].GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                var usage = commonFingerprints.Contains(vcol.CreateFingerprint()) ? DTSUsageType.UT_READONLY : leftOnlyColumnUsage;
                input.SetUsageType(vcol.LineageID, usage);
            }
            input = ComponentMetaData.InputCollection[MergingPropertyNames.InputProperties.RightId].GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
            {
                input.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
            }
        }

        protected MultipleValueDictionary<string, object[]> AppendsByCommonFieldHash { get; private set; }

        public override void PreExecute()
        {
            base.PreExecute();
            AppendsByCommonFieldHash = new MultipleValueDictionary<string, object[]>();
        }

        protected override void ProcessRightInput(IDTSInput100 input, PipelineBuffer buffer)
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
            NoCommonColumns = 9,
        }
    }
}
