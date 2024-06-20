using System.Text.RegularExpressions;
using RevolutionaryStuff.Core.Services.CodeStringGenerator;

namespace RevolutionaryStuff.Data.JsonStore.Entities;

internal partial class DefaultJsonEntityServices : IJsonEntityIdServices
{
    public static readonly IJsonEntityIdServices Instance = new DefaultJsonEntityServices();

    private IJsonEntityIdServices I => this;

    [GeneratedRegex(@"^[a-zA-Z0-9\-_\.@]+$")]
    private static partial Regex IdRegex();

    [GeneratedRegex(@"[^\w@.]")]
    private static partial Regex NonIdFriendlyCharacterExpr();

    private readonly ICodeStringGenerator CodeStringGenerator;

    public DefaultJsonEntityServices()
        : this(DefaultCodeStringGenerator.Instance)
    { }

    public DefaultJsonEntityServices(ICodeStringGenerator codeStringGenerator)
    {
        ArgumentNullException.ThrowIfNull(codeStringGenerator);
        CodeStringGenerator = codeStringGenerator;
    }

    private string CreateCode()
        => CodeStringGenerator.CreateRomanLowerCharactersCode(10);


    private void CheckTypeIsJsonEntity(Type entityDataType)
    {
        ArgumentNullException.ThrowIfNull(entityDataType);
        if (!entityDataType.IsA<JsonEntity>()) throw new ArgumentOutOfRangeException(nameof(entityDataType), $"Must be a subclass of {nameof(JsonEntity)}");
    }

    string IJsonEntityIdServices.CreateId(Type entityDataType, string? name, bool includeRandomCode)
    {
        CheckTypeIsJsonEntity(entityDataType);

        var abbreviation = GetAbbreviationFromDataType(entityDataType.FullName!);
        string id;
        name = name.TrimOrNull();
        if (name == null)
        {
            id = $"{abbreviation}";
        }
        else
        {
            var safeName = NonIdFriendlyCharacterExpr().Replace(name.RemoveDiacritics(), "");
            id = $"{abbreviation}-{safeName}";
        }
        if (includeRandomCode)
        { 
            id = $"{id}-{CreateCode()}";
        }
        return id.ToLower();
    }

    bool IJsonEntityIdServices.IsValid(Type entityDataType, string id)
    {
        try
        {
            I.ThrowIfInvalid(entityDataType, id);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    void IJsonEntityIdServices.ThrowIfInvalid(Type entityDataType, string id)
    {
        CheckTypeIsJsonEntity(entityDataType);

        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }
        if (id.Length < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be at least 3 characters long");
        }
        if (id.Length > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be no more than 128 characters long");
        }
        if (!IdRegex().IsMatch(id))
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "Id must match regex " + IdRegex());
        }
    }

    private static string GetAbbreviationFromDataType(string dataType)
    {
        var pos = dataType.LastIndexOf(JsonEntityPrefixAttribute.Separator);
        var abbreviation = dataType[(pos + 1)..];
        return abbreviation;
    }
}
