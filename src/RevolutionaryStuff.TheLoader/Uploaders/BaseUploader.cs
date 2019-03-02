using System;
using System.Data;
using System.Threading.Tasks;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.TheLoader.Uploaders
{
    public abstract class BaseUploader : IUploader
    {
        private readonly Program Program;

        protected BaseUploader(Program program)
        {
            Requires.NonNull(program, nameof(program));

            Program = program;
            OnInitProgramSettings(program);
        }

        protected virtual void OnInitProgramSettings(Program program)
        { }

        Task IUploader.UploadAsync(DataTable dt)
        {
            Requires.NonNull(dt, nameof(dt));
            return OnUploadAsync(dt);
        }

        protected abstract Task OnUploadAsync(DataTable dt);
    }
}
