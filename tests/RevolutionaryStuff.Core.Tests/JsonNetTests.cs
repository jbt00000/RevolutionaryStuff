using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class JsonNetTests
    {
        private static void TraceAsJson(object o)
            => Trace.WriteLine(JsonNetHelpers.ToJson(o));

        [TestMethod]
        public void SingleSegmentDueToWrongPathFormat()
        {
            Assert.AreEqual(1, JsonNetHelpers.DecomposePath("a/b/c/d", JsonNetHelpers.PathFormats.DotNotation).Length);
            Assert.AreEqual(1, JsonNetHelpers.DecomposePath("a.b.c.d", JsonNetHelpers.PathFormats.SlashNotation).Length);
        }

        [TestMethod]
        public void CorrectSegmentCountDueToCorrectPathFormat()
        {
            Assert.AreEqual(4, JsonNetHelpers.DecomposePath("a/b/c/d", JsonNetHelpers.PathFormats.SlashNotation).Length);
            Assert.AreEqual(4, JsonNetHelpers.DecomposePath("a.b.c.d", JsonNetHelpers.PathFormats.DotNotation).Length);
            Assert.AreEqual(4, JsonNetHelpers.DecomposePath("a/b/c/d", JsonNetHelpers.PathFormats.DotOrSlashNotation).Length);
            Assert.AreEqual(4, JsonNetHelpers.DecomposePath("a.b.c.d", JsonNetHelpers.PathFormats.DotOrSlashNotation).Length);
        }

        [TestMethod]
        public void PropertyPathSegment()
        {
            var segments = JsonNetHelpers.PathSegment.CreateSegmentsFromJsonPath("a");
            Assert.AreEqual(1, segments.Count);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Property, segments[0].SegmentType);
        }

        [TestMethod]
        public void ObjectPropertyPathSegment()
        {
            var segments = JsonNetHelpers.PathSegment.CreateSegmentsFromJsonPath("a.b");
            Assert.AreEqual(2, segments.Count);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Object, segments[0].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Property, segments[1].SegmentType);
        }

        [TestMethod]
        public void ObjectObjectPropertyPathSegment()
        {
            var segments = JsonNetHelpers.PathSegment.CreateSegmentsFromJsonPath("a.b.c");
            Assert.AreEqual(3, segments.Count);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Object, segments[0].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Object, segments[1].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Property, segments[2].SegmentType);
        }

        [TestMethod]
        public void ArrayIndexPathSegment()
        {
            var segments = JsonNetHelpers.PathSegment.CreateSegmentsFromJsonPath("a.4");
            Assert.AreEqual(2, segments.Count);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Array, segments[0].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.ArrayIndex, segments[1].SegmentType);
        }

        [TestMethod]
        public void ObjectObjectArrayIndexPathSegment()
        {
            var segments = JsonNetHelpers.PathSegment.CreateSegmentsFromJsonPath("a.b.c.4");
            Assert.AreEqual(4, segments.Count);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Object, segments[0].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Object, segments[1].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Array, segments[2].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.ArrayIndex, segments[3].SegmentType);
        }

        [TestMethod]
        public void ObjectObjectArrayIndexIndexPathSegment()
        {
            var segments = JsonNetHelpers.PathSegment.CreateSegmentsFromJsonPath("a.b.c.4.5");
            Assert.AreEqual(5, segments.Count);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Object, segments[0].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Object, segments[1].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.Array, segments[2].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.ArrayIndex, segments[3].SegmentType);
            Assert.AreEqual(JsonNetHelpers.PathSegment.SegmentTypes.ArrayIndex, segments[4].SegmentType);
        }

        [TestMethod]
        public void SetValueRootTest()
        {
            var j = JObject.Parse("{}");
            Assert.AreEqual(0, j.Count);
            j.SetValue("a", 1);
            TraceAsJson(j);
            Assert.AreEqual(1, j["a"].Value<int>());
            Assert.AreEqual(1, j.Count);
            j.SetValue("b", 2);
            TraceAsJson(j);
            Assert.AreEqual(2, j["b"].Value<int>());
            Assert.AreEqual(2, j.Count);
        }

        [TestMethod]
        public void OverwriteValueRootTest()
        {
            var j = JObject.Parse("{}");
            j.SetValue("a", 1);
            TraceAsJson(j);
            Assert.AreEqual(1, j["a"].Value<int>());
            j.SetValue("a", "aa");
            TraceAsJson(j);
            Assert.AreEqual("aa", j["a"].Value<string>());
            Assert.AreEqual(1, j.Count);
        }

        [TestMethod]
        public void SetChildObjectTest()
        {
            var j = JObject.Parse("{}");
            j.SetValue("a.b", "ab");
            TraceAsJson(j);
            Assert.AreEqual("ab", j["a"]["b"].Value<string>());
            Assert.AreEqual(1, j.Count);
            Assert.AreEqual(1, ((JContainer)j["a"]).Count);
        }

        [TestMethod]
        public void SetArrayItem0Test()
        {
            const string expected = "hello";
            var j = JObject.Parse("{}");
            j.SetValue("a.items.0", expected);
            TraceAsJson(j);
            var actual = j["a"]["items"][0].Value<string>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SetArrayItem1No0Test()
        {
            const string expected = "hello";
            var j = JObject.Parse("{}");
            j.SetValue("a.items.1", expected);
            TraceAsJson(j);
            var actual = j["a"]["items"][1].Value<string>();
            Assert.AreEqual(expected, actual);
            actual = j["a"]["items"][0].Value<string>();
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void SetArrayItem32Test()
        {
            const string expected = "hello";
            var j = JObject.Parse("{}");
            j.SetValue("a.items.3.2", expected);
            TraceAsJson(j);
            var actual = j["a"]["items"][3][2].Value<string>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SetPropertyOffObjectOffArrayTest()
        {
            const string expected = "hello";
            var j = JObject.Parse("{}");
            j.SetValue("a.items.3.first", expected);
            TraceAsJson(j);
            var actual = j["a"]["items"][3]["first"].Value<string>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Set2PropertiesOffObjectOffArrayTest()
        {
            var j = JObject.Parse("{}");
            j.SetValue("a.items.3.first", "f");
            j.SetValue("a.items.3.last", "l");
            TraceAsJson(j);
            var actual = j["a"]["items"][3]["first"].Value<string>();
            Assert.AreEqual("f", actual);
            actual = j["a"]["items"][3]["last"].Value<string>();
            Assert.AreEqual("l", actual);
        }
    }
}
