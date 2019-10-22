using System.IO;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Mergers.Pdf
{
    public interface IPdfMerger
    {
        Task<Stream> MergeAsync(PdfMergeInputs mergeInputs);
    }
}
