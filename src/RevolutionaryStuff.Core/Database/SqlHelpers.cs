namespace RevolutionaryStuff.Core.Database;

public static class SqlHelpers
{
    public const string DefaultSchema = "dbo";

    public static string SchemaOrDefault(string schemaName)
        => schemaName.TrimOrNull() ?? DefaultSchema;

    extension(string source)
    {
        public string EscapeForSql()
                => source.Replace("'", "''");
        public string PlaceInSqlStatement()
                => source == null ? " null " : " '" + source.EscapeForSql() + "' ";
    }



    extension(int? source)
    {
        public string PlaceInSqlStatement()
                => source == null ? " null " : $" {source} ";
    }

    extension(bool? source)
    {
        public string PlaceInSqlStatement()
                => source == null ? " null " : source.Value ? " 1 " : " 0 ";
    }
}
