using System.Collections.Generic;

namespace RevolutionaryStuff.Core.Data
{
    public interface IDataSet
    {
        string Name { get; set; }
        IList<IDataTable> Tables { get; }
    }
}
