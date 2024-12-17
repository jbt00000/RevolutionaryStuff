namespace RevolutionaryStuff.Core;
public partial class JsonHelpers
{
    public enum PathFormats
    {
        DotNotation,
        SlashNotation,
        DotOrSlashNotation,
        Default = DotOrSlashNotation
    }

    public static string[] DecomposePath(string path, PathFormats pathFormat = PathFormats.DotNotation)
    {
        return pathFormat switch
        {
            PathFormats.DotNotation => path.Split('.'),
            PathFormats.SlashNotation => path.Split('/'),
            PathFormats.DotOrSlashNotation => path.Split('.', '/'),
            _ => throw new UnexpectedSwitchValueException(pathFormat),
        };
    }
    public class PathSegment
    {
        public override string ToString()
            => $"{Name ?? Index.ToString()} is {SegmentType}";

        public enum SegmentTypes
        {
            Array,
            Property,
            Object,
            ArrayIndex,
        }
        public SegmentTypes SegmentType;
        public string Name;
        public int Index;

        public static IList<PathSegment> CreateSegmentsFromJsonPath(string jsonPath, PathFormats pathFormat = PathFormats.DotNotation)
            => CreateSegmentsFromPathParts(DecomposePath(jsonPath, pathFormat));

        public static IList<PathSegment> CreateSegmentsFromPathParts(IList<string> parts)
        {
            var segments = parts.ConvertAll(part => new PathSegment { Name = part, SegmentType = SegmentTypes.Object });
            PathSegment sn = null;
            for (var z = segments.Count - 1; z >= 0; --z)
            {
                var s = segments[z];
                if (int.TryParse(s.Name, out var i))
                {
                    s.SegmentType = SegmentTypes.ArrayIndex;
                    s.Index = i;
                    s.Name = null;
                }
                else
                {
                    s.SegmentType = sn == null ? SegmentTypes.Property : sn.SegmentType == SegmentTypes.ArrayIndex ? SegmentTypes.Array : SegmentTypes.Object;
                }
                sn = s;
            }
            return segments;
        }

        public static string CreateJsonPointerPath(IList<PathSegment> segments)
        {
            List<string> parts = [];
            foreach (var segment in segments)
            {
                if (segment.SegmentType == JsonHelpers.PathSegment.SegmentTypes.ArrayIndex)
                {
                    parts.Add(segment.Index.ToString());
                }
                else
                {
                    parts.Add(segment.Name);
                }
            }
            return "/" + parts.WhereNotNull().Join("/");
        }
    }
}
