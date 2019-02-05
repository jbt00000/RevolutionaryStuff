namespace RevolutionaryStuff.Core.Database
{
    public static class SqlHelpers
    {
        public const string DefaultSchema = "dbo";

        public static string SchemaOrDefault(string schemaName)
            => StringHelpers.TrimOrNull(schemaName) ?? DefaultSchema;

        /// <summary>
        /// Remove single quotes within the text for SQL
        /// </summary>
        public static string EscapeForSql(this string source)
            => source.Replace("\'", "");
    }
}
