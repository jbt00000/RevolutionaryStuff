using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.Cosmos;

public class CosmosEntity<TPrimaryKey> : IPrimaryKey<TPrimaryKey>, IETagGetter
{
    [JsonProperty(CosmosEntityPropertyNames.Id)]
    public TPrimaryKey Id { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }

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
