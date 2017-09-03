using Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    public interface IDtsColumn
    {
        bool IsInputColumn { get; }
        bool IsOutputColumn { get; }
        string Name { get; }
        DataType DataType { get; }
    }
}
