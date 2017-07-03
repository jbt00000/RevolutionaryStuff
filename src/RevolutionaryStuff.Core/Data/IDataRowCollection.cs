namespace RevolutionaryStuff.Core.Data
{
    public interface IDataRowCollection
    {
        int Count { get; }
        IDataRow this[int i] { get; }
        void Add(object[] fields);
        void Add(IDataRow dataRow);
    }
}
