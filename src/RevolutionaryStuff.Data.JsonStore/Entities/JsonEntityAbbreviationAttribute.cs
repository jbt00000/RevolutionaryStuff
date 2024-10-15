namespace RevolutionaryStuff.Data.JsonStore.Entities;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class JsonEntityAbbreviationAttribute : Attribute
{
    private readonly string Abbreviation;

    public JsonEntityAbbreviationAttribute(string abbreviation)
    {
        Abbreviation = abbreviation;
    }

    public static string GetAbbreviation<TEntity>()
        => GetAbbreviation(typeof(TEntity));

    public static string GetAbbreviation(Type tEntity)
    {
        JsonEntity.ThrowIfNotJsonEntity(tEntity);
        var abbr = tEntity.GetCustomAttribute<JsonEntityAbbreviationAttribute>()?.Abbreviation;
        if (string.IsNullOrEmpty(abbr))
        {
            throw new Exception($"The type {tEntity.Name} does not have a {nameof(JsonEntityAbbreviationAttribute)}.");
        }
        return abbr;
    }
}
