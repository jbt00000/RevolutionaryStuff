using System.Collections.Generic;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    public interface IComponentHost
    {
        IDictionary<string, object> Properties { get; }

        T Use<T>(params object[] args);
    }
}
