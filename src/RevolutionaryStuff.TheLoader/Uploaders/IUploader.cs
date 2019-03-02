using System.Data;
using System.Threading.Tasks;

namespace RevolutionaryStuff.TheLoader.Uploaders
{
    public interface IUploader
    {
        Task UploadAsync(DataTable dt);
    }
}
