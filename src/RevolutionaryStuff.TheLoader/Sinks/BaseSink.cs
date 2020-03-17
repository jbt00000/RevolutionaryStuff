using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.TheLoader.Uploaders;

namespace RevolutionaryStuff.TheLoader.Sinks
{
    public abstract class BaseSink : ISink
    {
        private readonly Program Program;
        protected readonly IOptions<Program.LoaderConfig.TableConfig> TableConfigOptions;

        protected BaseSink(Program program, IOptions<Program.LoaderConfig.TableConfig> tableConfigOptions)
        {
            Requires.NonNull(program, nameof(program));

            Program = program;
            TableConfigOptions = tableConfigOptions;
            OnInitProgramSettings(program);
        }

        protected virtual void OnInitProgramSettings(Program program)
        { }

        Task ISink.UploadAsync(DataTable dt)
        {
            Requires.NonNull(dt, nameof(dt));
            return OnUploadAsync(dt);
        }

        protected abstract Task OnUploadAsync(DataTable dt);
    }
}
