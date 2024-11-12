using System.Text.Json;

namespace RevolutionaryStuff.Core.ApplicationParts;

public interface IJsonSerializable
{
    string ToJson();

    #region Default Implementation

    JsonElement ToJsonElement()
        => JsonDocument.Parse(ToJson()).RootElement;

    #endregion
}
