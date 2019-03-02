using System.Data;
using System.IO;
using System.Threading.Tasks;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.TheLoader.Uploaders
{
    public class FlatFileUploader : BaseUploader
    {
        public FlatFileUploader(Program program)
            : base(program)
        { }

        protected string OutputFilename;
        protected char FieldDelimChar;

        protected override void OnInitProgramSettings(Program program)
        {
            base.OnInitProgramSettings(program);
            OutputFilename = program.ConnectionString;
            FieldDelimChar = program.SinkCsvFieldDelim;
        }

        protected override async Task OnUploadAsync(DataTable dt)
        {
            using (var sw = File.CreateText(Path.GetFullPath(OutputFilename)))
            {
                await dt.ToCsvAsync(sw, FieldDelimChar);
            }
        }
    }
}
