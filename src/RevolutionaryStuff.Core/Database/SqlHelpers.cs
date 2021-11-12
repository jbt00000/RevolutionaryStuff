namespace RevolutionaryStuff.Core.Database;

public static class SqlHelpers
{
    public const string DefaultSchema = "dbo";

    public static string SchemaOrDefault(string schemaName)
        => StringHelpers.TrimOrNull(schemaName) ?? DefaultSchema;

    public static string EscapeForSql(this string source)
        => source.Replace("'", "''");

    public static string PlaceInSqlStatement(this string source)
        => source == null ? " null " : " '" + source.EscapeForSql() + "' ";

    public static string PlaceInSqlStatement(this int? source)
        => source == null ? " null " : $" {source} ";

    public static string PlaceInSqlStatement(this bool? source)
        => source == null ? " null " : source.Value ? " 1 " : " 0 ";
}
