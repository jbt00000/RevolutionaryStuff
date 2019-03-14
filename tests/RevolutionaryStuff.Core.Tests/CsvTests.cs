using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests
{
    [TestClass]
    public class CsvTests
    {
        [TestMethod]
        public void FormatCellNoEscapeSingleChar()
        {
            Assert.AreEqual("a", CSV.Format("a"));
        }

        [TestMethod]
        public void FormatCellNoEscapeMultipleChars()
        {
            Assert.AreEqual("aa", CSV.Format("aa"));
        }

        [TestMethod]
        public void FormatCellNoEscapeMultiplePreSpace()
        {
            Assert.AreEqual(" a", CSV.Format(" a"));
        }

        [TestMethod]
        public void FormatCellNoEscapeMultiplePostSpace()
        {
            Assert.AreEqual("a ", CSV.Format("a "));
        }

        [TestMethod]
        public void FormatCellNoEscapeMultiplePreTab()
        {
            Assert.AreEqual("a\t", CSV.Format("a\t"));
        }

        [TestMethod]
        public void FormatCellNoEscapeMultiplePostTab()
        {
            Assert.AreEqual("\ta", CSV.Format("\ta"));
        }

        [TestMethod]
        public void FormatCellWithNewlineR()
        {
            Assert.AreEqual("\"a\rb\"", CSV.Format("a\rb"));
        }

        [TestMethod]
        public void FormatCellWithNewlineN()
        {
            Assert.AreEqual("\"a\nb\"", CSV.Format("a\nb"));
        }

        [TestMethod]
        public void FormatCellWithEmbeddedCommaWithCommaDelim()
        {
            Assert.AreEqual("\"a,b\"", CSV.Format("a,b"));
        }

        [TestMethod]
        public void FormatEvilLineDelimComma()
        {
            Assert.AreEqual(
                @"a,"","", b,c ,d d,""e,e"",""f" + "\n" + @"f"",g|g",
                CSV.FormatLine(new[] { "a", ",", " b", "c ", "d d", "e,e", "f\nf", "g|g" },
                false));
        }

        [TestMethod]
        public void FormatEvilLineDelimPipe()
        {
            var sb = new StringBuilder();
            CSV.FormatLine(sb, new[] { "a", ",", " b", "c ", "d d", "e,e", "f\nf", "g|g" }, false, '|');
            var actual = sb.ToString();
            Assert.AreEqual(@"a|,| b|c |d d|e,e|""f" + "\n" + @"f""|""g|g""", actual);
        }
    }
}
