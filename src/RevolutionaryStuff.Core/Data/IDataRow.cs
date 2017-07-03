using System.Data;

namespace RevolutionaryStuff.Core.Data
{
    public interface IDataRow : IDataRecord
    {
        IDataTable Table { get; }
        new object this[string name] { get; set; }
        new object this[int i] { get; set; }
    }
}
