using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace RevolutionaryStuff.Core.ApplicationParts;

public interface IJsonSerializable
{
    string ToJson();

    #region Default Implementation

    JObject ToJObject()
        => JObject.Parse(ToJson());

    JsonElement ToJsonElement()
        => JsonDocument.Parse(ToJson()).RootElement;

    #endregion
}
