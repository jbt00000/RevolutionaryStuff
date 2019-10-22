using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Mergers;
using RevolutionaryStuff.Mergers.Pdf;

namespace RevolutionaryStuff.Functions.Merger.PdfMergerTests
{
    [TestClass]
    public partial class PdfTests
    {
        private readonly IServiceProvider SP;

        public PdfTests()
        {
            SP = TestFixture.Instance.CreateScopedProvider();
        }

        [TestMethod]
        public async Task GoAsync()
        {
            var merger = SP.GetRequiredService<IPdfMerger>();
            using (var resp = await merger.MergeAsync(new PdfMergeInputs
            {
                Template = new TemplateInfo
                {
                    TemplateStream = RevolutionaryStuff.Core.ResourceHelpers.GetEmbeddedResourceAsStream(typeof(PdfTests).Assembly, "a.pdf")
                },
                Data = new MergeDataInfo
                {
                    FieldDatas = new[] {
                        new FieldData("insuredFullName", "Jason B Thomas"),
                    }
                }
            }))
            {
                Assert.IsNotNull(resp, nameof(resp));
            }
        }
    }
}
