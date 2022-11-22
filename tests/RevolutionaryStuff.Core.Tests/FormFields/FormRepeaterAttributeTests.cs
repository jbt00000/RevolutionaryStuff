using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.FormFields;

namespace RevolutionaryStuff.Core.Tests.FormFields
{
    [TestClass]
    public class FormRepeaterAttributeTests
    {
        [TestMethod]
        public void TransformWithBasisTest()
        {
            var fra = new FormFieldRepeaterAttribute($"PRE.{FormFieldContainerAttribute.FieldNameToken}.POST.{FormFieldRepeaterAttribute.IndexToken}", 33);
            Assert.AreEqual("PRE.fff.POST.50", fra.TransformName("fff", 17));
        }
    }
}
