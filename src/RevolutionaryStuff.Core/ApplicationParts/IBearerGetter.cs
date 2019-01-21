using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    public interface IBearerGetter
    {
        Task<string> GetBearerAsync(string name=null);
    }
}
