namespace RevolutionaryStuff.Data.ETL;

public interface IFixedWidthColumnInfo
{
    string ColumnName { get; }
    int StartAt { get; }
    int? EndAt { get; }
    int? Length { get; }
    Type DataType { get; }
}
