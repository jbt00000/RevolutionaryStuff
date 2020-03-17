using System.Data;
using System.Threading.Tasks;

namespace RevolutionaryStuff.TheLoader.Uploaders
{
    public interface ISink
    {
        Task UploadAsync(DataTable dt);
    }
}
