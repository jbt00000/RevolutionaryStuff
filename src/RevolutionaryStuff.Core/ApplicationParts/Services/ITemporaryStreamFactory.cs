using System.IO;

namespace RevolutionaryStuff.Core.ApplicationParts.Services
{
    public interface ITemporaryStreamFactory
    {
        Stream Create(int? capacity = null);
    }
}
