using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.FormFields;

namespace RevolutionaryStuff.Core.Tests.FormFields
{
    [TestClass]
    public class FormFieldSerializationTests
    {
        private const string PersonTypeArtificialLabel = "artificial";

        public enum PersonType
        {
            Bio,
            [EnumeratedStringValue(PersonTypeArtificialLabel)]
            AI
        }

        private static void AssertSingle(string expectedKey, object expectedVal, IEnumerable<KeyValuePair<string, object>> kvps)
        {
            var found = 0;
            foreach (var kvp in kvps)
            {
                if (kvp.Key == expectedKey)
                {
                    //broken into 2 statements to ease with setting breakpoints
                    if (kvp.Value == expectedVal)
                    {
                        ++found;
                    }
                    else if (expectedVal != null && expectedVal.Equals(kvp.Value))
                    {
                        ++found;
                    }
                }
            }
            Assert.AreEqual(1, found);
        }

        public class BoolTransformedClass
        {
            [BooleanTransformedFormField("yes", "no")]
            [FormField("f1")]
            public bool Yes { get; set; }

            [BooleanTransformedFormField("yes", "no")]
            [FormField("f2")]
            public bool No { get; set; }

            [BooleanTransformedFormField("1", "2", "3")]
            [FormField("f3")]
            public bool? YesNoNull { get; set; }
        }

        public class SimpleClass
        {
            public string Field0;

            public string Field1;

            public string Prop0 { get; set; }

            [FormField("P1")]
            public string Prop1 { get; set; }
        }

        public class SimpleClassWithList<T> : SimpleClass
        {
            [FormFieldRepeater("item{I}{N}")]
            public IList<T> Items { get; set; }
        }

        public class SimpleClassDefaultWithList<T> : SimpleClass
        {
            [FormFieldRepeater("item")]
            public IList<T> Items { get; set; }
        }

        [TestMethod]
        public void BoolTransformedTest()
        {
            var ret = FormFieldHelpers.ConvertObjectToKeyValuePairs(new BoolTransformedClass
            {
                Yes = true,
                No = false,
                YesNoNull = null,
            });
            Assert.IsNotNull(ret);
            Assert.AreEqual(3, ret.Count());
            AssertSingle("f1", "yes", ret);
            AssertSingle("f2", "no", ret);
            AssertSingle("f3", "3", ret);
        }


        [TestMethod]
        public void ConvertNull()
        {
            var ret = FormFieldHelpers.ConvertObjectToKeyValuePairs(null);
            Assert.IsNotNull(ret);
            Assert.AreEqual(0, ret.Count());
        }

        [TestMethod]
        public void SerializeSimpleClassAllNulls()
        {
            var ret = FormFieldHelpers.ConvertObjectToKeyValuePairs(new SimpleClass());
            Assert.IsNotNull(ret);
            Assert.AreEqual(0, ret.Count());
        }

        [TestMethod]
        public void SerializeSimpleClassAllFilled()
        {
            var ret = FormFieldHelpers.ConvertObjectToKeyValuePairs(new SimpleClass()
            {
                Field0 = "f0",
                Field1 = "f1",
                Prop0 = "p0",
                Prop1 = "p1",
            });
            Assert.IsNotNull(ret);
            Assert.AreEqual(1, ret.Count());
            AssertSingle("P1", "p1", ret);
        }

        [TestMethod]
        public void SerializeSimpleClassWithStringList()
        {
            var c = new SimpleClassWithList<string>
            {
                Prop1 = "p1",
                Items = new[] {
                    "a",
                    "b"
                }
            };
            var ret = FormFieldHelpers.ConvertObjectToKeyValuePairs(c);
            Assert.IsNotNull(ret);
            Assert.AreEqual(3, ret.Count());
            AssertSingle("P1", "p1", ret);
            AssertSingle("item0", "a", ret);
            AssertSingle("item1", "b", ret);
        }

        [TestMethod]
        public void SerializeSimpleClassSmartPatternWithStringList()
        {
            var c = new SimpleClassDefaultWithList<string>
            {
                Prop1 = "p1",
                Items = new[] {
                    "a",
                    "b"
                }
            };
            var ret = FormFieldHelpers.ConvertObjectToKeyValuePairs(c);
            Assert.IsNotNull(ret);
            Assert.AreEqual(3, ret.Count());
            AssertSingle("P1", "p1", ret);
            AssertSingle("item0", "a", ret);
            AssertSingle("item1", "b", ret);
        }

        public class Person
        {
            [FormField("firstName")]
            public string First { get; set; }

            [FormField("lastName")]
            public string Last { get; set; }

            [FormField("personType")]
            public PersonType PT { get; set; }



            public Person(string first, string last, PersonType pt)
            {
                First = first;
                Last = last;
                PT = pt;
            }
        }

        private static SimpleClassWithList<Person> CreateTestPersonList()
            => new()
            {
                Prop1 = "p1",
                Items = new[] {
                    new Person("jason", "thomas", PersonType.Bio),
                    new Person("joe", "blow", PersonType.AI)
                }
            };

        [TestMethod]
        public void SerializeSimpleClassWithPersonList()
        {
            var c = CreateTestPersonList();
            var ret = FormFieldHelpers.ConvertObjectToKeyValuePairs(c);
            Assert.IsNotNull(ret);
            Assert.AreEqual(7, ret.Count());
            AssertSingle("P1", "p1", ret);
            AssertSingle("item0firstName", "jason", ret);
            AssertSingle("item0lastName", "thomas", ret);
            AssertSingle("item0personType", PersonType.Bio, ret);
            AssertSingle("item1firstName", "joe", ret);
            AssertSingle("item1lastName", "blow", ret);
            AssertSingle("item1personType", PersonType.AI, ret);
        }

        [TestMethod]
        public void SerializeSimpleClassWithPersonListEnumAsString()
        {
            var c = CreateTestPersonList();
            var ret = FormFieldHelpers.ConvertObjectToKeyValuePairs(c, new FormFieldHelpers.ConversionSettings { EnumerationSerializationOption = FormFieldHelpers.EnumerationSerializationOptions.AsString });
            AssertSingle("item0personType", PersonType.Bio.ToString(), ret);
            AssertSingle("item1personType", PersonTypeArtificialLabel, ret);
        }

        [TestMethod]
        public void SerializeSimpleClassWithPersonListEnumAsNumber()
        {
            var c = CreateTestPersonList();
            var ret = FormFieldHelpers.ConvertObjectToKeyValuePairs(c, new FormFieldHelpers.ConversionSettings { EnumerationSerializationOption = FormFieldHelpers.EnumerationSerializationOptions.AsNumber });
            AssertSingle("item0personType", (int)(object)PersonType.Bio, ret);
            AssertSingle("item1personType", (int)(object)PersonType.AI, ret);
        }
    }
}
