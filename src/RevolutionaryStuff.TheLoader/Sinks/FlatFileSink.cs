using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.TheLoader.Sinks
{
    public class FlatFileSink : BaseSink
    {
        public FlatFileSink(Program program, IOptions<Program.LoaderConfig.TableConfig> tableConfigOptions)
            : base(program, tableConfigOptions)
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
