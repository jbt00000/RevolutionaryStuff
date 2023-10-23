using System.IO;

namespace RevolutionaryStuff.Core.Services.TemporaryStreamFactory;

public interface ITemporaryStreamFactory
{
    Stream Create(long? capacity = null);
}
