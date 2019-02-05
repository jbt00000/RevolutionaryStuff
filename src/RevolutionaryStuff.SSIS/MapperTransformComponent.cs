using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.SSIS
{
    [DtsPipelineComponent(
        DisplayName = "The Mapper",
        ComponentType = ComponentType.Transform,
        SupportsBackPressure = true,
        NoEditor = false,
        CurrentVersion = BasePipelineComponent.AssemblyComponentVersion,
        IconResource = "RevolutionaryStuff.SSIS.Resources.FavIcon.ico")]
    public class MapperTransformComponent : BaseMergingComponent
    {
        private static class PropertyNames
        {
            public const string IgnoreCase = CommonPropertyNames.IgnoreCase;
            public const string Mappings = "Mappings";
            public const string RightMap = "RightMap";
            public static class OutputProperties
            {
                public const string Matches = "Match Output";
            }
        }

        public MapperTransformComponent()
            : base(true)
        { }

        protected override void OnProvideComponentProperties(IDTSInput100 leftInput, IDTSInput100 rightInput, IDTSOutput100 primaryOutput)
        {
            ComponentMetaData.Name = "The Mapper";
            ComponentMetaData.Description = "A SSIS Data Flow Transformation Component that maps keys from the right table onto the left based off of join conditions.";

            CreateCustomProperty(PropertyNames.RightMap, null, "LookupVal,LookupKey1,...LookupKeyN");
            CreateCustomProperty(PropertyNames.Mappings, null, "Lookup1ResultFieldName,Lookup1Key1,...Lookup1KeyN;LookupNResultFieldName,LookupNKey1,...LookupNKeyN;");
            CreateCustomProperty(PropertyNames.IgnoreCase, "1", "When {1,true} the match is case insensitive, when {0,false} it is case sensitive.");

            primaryOutput.SynchronousInputID = leftInput.ID;
            primaryOutput.Name = "Match Output";
            primaryOutput.Description = "Root rows that have have corresponding matches in the Comparison";
        }

        IList<string> GetRightMap()
            => CSV.ParseLine(GetCustomPropertyAsString(PropertyNames.RightMap) ?? "").Select(z => StringHelpers.TrimOrNull(z)).Where(z => z != null).ToList();
        
        IDictionary<string, IList<string>> GetLeftMaps()
        {
            var p = ComponentMetaData.CustomPropertyCollection[PropertyNames.Mappings];
            var d = new Dictionary<string, IList<string>>();
            var csv = (p.Value as string ?? "").Replace(";", "\n");
            foreach (var row in CSV.ParseText(csv))
            {
                if (row.Length < 2) continue;
                d[row[0]] = new List<string>(row.Skip(1)).AsReadOnly();
            }
            return d;
        }

        public override IDTSCustomProperty100 SetComponentProperty(string propertyName, object propertyValue)
        {
            switch (propertyName)
            {
                case PropertyNames.OutputProperties.Matches:
                    ComponentMetaData.OutputCollection[0].OutputColumnCollection.AddOutputColumns(ComponentMetaData.InputCollection[0].InputColumnCollection);
                    break;
            }
            return base.SetComponentProperty(propertyName, propertyValue);
        }

        protected override void DefineOutputs(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IList<string> commonFingerprints)
        {
            var m = GetLeftMaps();
            var inputColNames = new HashSet<string>(Comparers.CaseInsensitiveStringComparer);
            foreach (var kvp in m)
            {
                foreach (var colName in kvp.Value)
                {
                    inputColNames.Add(colName);
                }
            }
            for (int z = 0; z < 2; ++z)
            {
                var virtualInputs = ComponentMetaData.InputCollection[z].GetVirtualInput();
                foreach (IDTSVirtualInputColumn100 vcol in virtualInputs.VirtualInputColumnCollection)
                {
                    virtualInputs.SetUsageType(vcol.LineageID, inputColNames.Contains(vcol.Name) || z==1 ? DTSUsageType.UT_READONLY : DTSUsageType.UT_IGNORED);
                }
            }
            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var rmap = GetRightMap();
            var rightKeyCol = ComponentMetaData.InputCollection[1].InputColumnCollection.FindByName(rmap[0]);
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            outCols.RemoveAll();
            foreach (var key in m.Keys)
            {
                outCols.AddOutputColumn(rightKeyCol, key);
            }
        }

        protected override DTSValidationStatus OnValidate(IDTSInputColumnCollection100 leftColumns, IDTSInputColumnCollection100 rightColumns, IDTSOutputColumnCollection100 outputColumns, IList<string> commonFingerprints)
        {
            var ret = base.OnValidate(leftColumns, rightColumns, outputColumns, commonFingerprints);
            if (ret != DTSValidationStatus.VS_ISVALID) return ret;
            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
            var outCols = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
            var m = GetLeftMaps();
            if (outCols.Count != m.Count)
            {
                ret = DTSValidationStatus.VS_ISBROKEN;
            }
            return ret;
        }

        public override void PreExecute()
        {
            base.PreExecute();

            foreach (var kvp in GetLeftMaps())
            {
                FireInformation(InformationMessageCodes.LeftMap, $"{kvp.Key}=>{CSV.FormatLine(kvp.Value, false)}");
            }
            var r = GetRightMap();
            FireInformation(InformationMessageCodes.RightMap, $"{r[0]}=>{CSV.FormatLine(r.Skip(1), false)}");

            LeftSamples = 0;
            RightSamples = 0;
        }

        private class MyRuntimeData : MergingRuntimeData
        {
            protected new MapperTransformComponent Parent
                => (MapperTransformComponent)base.Parent;

            public readonly IDictionary<string, object> ValByKey;

            protected override void OnDispose(bool disposing)
            {
                base.OnDispose(disposing);
                ValByKey.Clear();
            }

            public MyRuntimeData(MapperTransformComponent parent)
                : base(parent)
            {
                if (GetCustomPropertyAsBool(PropertyNames.IgnoreCase, true))
                {
                    ValByKey = new Dictionary<string, object>(Comparers.CaseInsensitiveStringComparer);
                }
                else
                {
                    ValByKey = new Dictionary<string, object>();
                }
            }
        }

        protected override RuntimeData ConstructRuntimeData()
            => new MyRuntimeData(this);

        private new MyRuntimeData RD
            => (MyRuntimeData)base.RD;


        private int ProcessInputRootHits = 0;
        private int ProcessInputRootMisses = 0;

        protected override void ProcessLeftInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            if (!ComponentMetaData.OutputCollection[0].IsAttached) return;
            var m = GetLeftMaps();

            var leftCols = ComponentMetaData.InputCollection[0].InputColumnCollection;

            int rowsProcessed = 0;
            var keys = new List<object>();
            while (buffer.NextRow())
            {
                foreach (var kvp in m)
                {
                    keys.Clear();
                    foreach (var fn in kvp.Value)
                    {
                        object o;
                        if (fn.StartsWith(":"))
                        {
                            o = fn.Substring(1);
                        }
                        else
                        {
                            o = GetObject(fn, buffer, RD.LeftInputCbm);
                        }
                        keys.Add(o);
                    }
                    var key = Cache.CreateKey(keys);
                    object val;
                    bool hit;
                    if (RD.ValByKey.TryGetValue(key, out val))
                    {
                        hit = true;
                        ++ProcessInputRootHits;
                        var outCol = RD.PrimaryOutputCbm.GetColumnFromColumnName(kvp.Key);
                        buffer.SetObject(outCol.DataType, RD.PrimaryOutputCbm.GetPositionFromColumnName(kvp.Key), val);
                    }
                    else
                    {
                        hit = false;
                        ++ProcessInputRootMisses;
                        buffer.SetNull(RD.PrimaryOutputCbm.GetPositionFromColumnName(kvp.Key));
                        val = null;
                    }
                    if (LeftSamples*m.Count < SampleSize)
                    {
                        ++LeftSamples;
                        FireInformation(InformationMessageCodes.LeftSample, hit ? $"{val}=>{key}" : $"<MISS>=>{key}");
                    }
                }
                ++rowsProcessed;
            }
            FireInformation(InformationMessageCodes.RowsProcessed, $"{rowsProcessed}");
            FireInformation(InformationMessageCodes.MatchStats, $"hits={ProcessInputRootHits}, misses={ProcessInputRootMisses}");
        }

        private int RightSamples;
        private int LeftSamples;

        protected override void ProcessRightInput(IDTSInput100 input, PipelineBuffer buffer)
        {
            int rowsProcessed = 0;
            var keys = new List<object>();
            var rmap = GetRightMap();
            while (buffer.NextRow())
            {
                keys.Clear();
                var idCol = rmap.First();
                var idVal = GetObject(rmap[0], buffer, RD.RightInputCbm);
                for (int z = 1; z < rmap.Count; ++z)
                {
                    var o = GetObject(rmap[z], buffer, RD.RightInputCbm);
                    keys.Add(o);
                }
                var key = Cache.CreateKey(keys);
                RD.ValByKey[key] = idVal;
                ++rowsProcessed;
                if (RightSamples < SampleSize)
                {
                    ++RightSamples;
                    FireInformation(InformationMessageCodes.RightSample, $"{idVal}=>{key}");
                }
            }
            FireInformation(InformationMessageCodes.RowsProcessed, $"{rowsProcessed}");
        }

        private enum InformationMessageCodes
        {
            RowsProcessed = 1,
            RightSample = 2,
            LeftSample = 3,
            MatchStats = 4,
            CommonColumns = 5,
            LeftColumns = 6,
            RightColumns = 7,
            LeftMap = 8,
            RightMap = 9,
        }
    }
}
