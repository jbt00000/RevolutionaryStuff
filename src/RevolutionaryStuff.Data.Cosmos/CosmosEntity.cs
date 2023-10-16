using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.Cosmos;

public class CosmosEntity<TPrimaryKey> : IPrimaryKey<TPrimaryKey>, IETagGetter
{
    [JsonPropertyName(CosmosEntityPropertyNames.Id)]
    public TPrimaryKey Id { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement> AdditionalData { get; set; }

    #region Cosmos Managed Properties

    [JsonIgnore]
    public string ETag
        => AdditionalData.GetString(CosmosEntityPropertyNames.ETag);

    string? IETagGetter.ETag
        => ETag;

    [JsonIgnore]
#pragma warning disable IDE1006 // Naming Styles
    public int _Timestamp
#pragma warning restore IDE1006 // Naming Styles
        => AdditionalData.GetInt(CosmosEntityPropertyNames.Timestamp);

    #endregion

    TPrimaryKey IPrimaryKey<TPrimaryKey>.Key
        => Id;

    object IPrimaryKey.Key
        => Id;
}
