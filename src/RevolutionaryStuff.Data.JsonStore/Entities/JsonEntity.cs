﻿using System.ComponentModel.DataAnnotations;
using System.IO.Hashing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.JsonSerializers.Microsoft;
using RevolutionaryStuff.Core.Services.Tenant;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Entities;

public abstract partial class JsonEntity : JsonSerializable, IPreSave, IValidate, IPrimaryKey<string>, IETagGetter
{
    public static IJsonEntityIdServices JsonEntityIdServices { get; internal set; } = DefaultJsonEntityServices.Instance;

    protected virtual string EntityDataTypeSuffix
        => JsonEntityAbbreviationAttribute.GetAbbreviation(GetType());

    protected virtual string? OnGetETag()
        => null;

    [JsonIgnore]
    public string? ETag
        => OnGetETag();

    public static class JsonEntityPropertyNames
    {
        public const string Id = "id";

        public const string SoftDeletedAt = "softDeletedAt";

        public const string TenantId = "tenantId";

        public const string DataType = "$type";

        public const string PartitionKey = "sk";

        public const string TouchedAt = "touchedAt";
    }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; set; }

    public void SetAdditionalData(string name, object val)
    {
        AdditionalData ??= new Dictionary<string, JsonElement>();
        var jel = val is JsonElement ? (JsonElement)val : SystemTextJsonSerializer.Instance.ToJsonElement(val);
        AdditionalData[name] = jel;
    }

    public bool TryGetAdditionalDataPoco<TPoco>(string key, out TPoco? poco)
        where TPoco : class
    {
        poco = default;
        if (key == null || AdditionalData == null || !AdditionalData.ContainsKey(key)) return false;
        var jel = AdditionalData[key];
        poco = JsonHelpers.FromJsonElement<TPoco>(jel);
        return true;
    }

    [Key] //This is required by ODataSources
    [JsonPropertyName(JsonEntityPropertyNames.Id)]
    public string Id { get; set; }

    [JsonPropertyName(JsonEntityPropertyNames.SoftDeletedAt)]
    public DateTimeOffset? SoftDeletedAt { get; set; }

    [JsonIgnore]
    public bool IsSoftDeleted => SoftDeletedAt != null;

    /// <remarks>Should be the first property to speed deserialization when using JsonPolymorphic</remarks>
    [JsonPropertyOrder(int.MinValue)]
    [JsonPropertyName(JsonEntityPropertyNames.DataType)]
    public string DataType { get; set; }

    [JsonPropertyName(JsonEntityPropertyNames.PartitionKey)]
    public string? PartitionKey { get; set; }

    protected JsonEntity()
    {
        var t = GetType();
        DataType = JsonEntityPrefixAttribute.GetPrefix(t) + JsonEntityPrefixAttribute.Separator + EntityDataTypeSuffix;
        Id = CreateId();
    }

    public static void ThrowIfNotJsonEntity(Type t)
    {
        ArgumentNullException.ThrowIfNull(t);
        if (!t.IsA<JsonEntity>())
        {
            throw new ArgumentOutOfRangeException(nameof(t), $"Type {t.Name} is not a subclass of {nameof(JsonEntity)}");
        }
    }

    protected string CreateId(string? name = null, bool includeRandomCode = true)
        => JsonEntityIdServices.CreateId(GetType(), name, includeRandomCode);

    public override string ToString()
        => $"id=[{Id}] dt=[{DataType}] type=[{GetType().Name}]";

    private static readonly IDictionary<Type, string> DataTypeByTypeDictionary = new System.Collections.Concurrent.ConcurrentDictionary<Type, string>();

    public static string GetDataType(Type t)
#pragma warning disable CS8603 // Possible null reference return.
        => DataTypeByTypeDictionary.FindOrCreate(t, () =>
        {
            try
            {
                return (t.Construct() as JsonEntity)?.DataType;
            }
            catch (Exception)
            {
                return null;
            }
        });
#pragma warning restore CS8603 // Possible null reference return.

    public static Type GetTypeByDataType(string dataType)
        => DataTypeByTypeDictionary.FirstOrDefault(z => z.Value == dataType).Key;

    public static string GetDataType<TEntity>() where TEntity : JsonEntity
        => GetDataType(typeof(TEntity));

    public const string VariableEntityDataType = "?";

    public static void InitDataTypes(Assembly a)
        => a.GetTypes()
            .Where(t => !t.IsAbstract && t.IsA<JsonEntity>())
            .ForEach(t => JsonEntity.GetDataType(t));

    void IPreSave.PreSave()
        => OnPreSave();

    protected virtual void OnPreSave()
    {
        var pka = GetType().GetCustomAttribute<JsonEntityPartitionKeyBaseAttribute>();
        if (pka != null)
        {
            switch (pka.PartitionKeyScheme)
            {
                case JsonEntityPartitionKeyBaseAttribute.PartitionKeySchemeEnum.SameAsObjectId:
                    if (PartitionKey == null)
                    {
                        PartitionKey = Id;
                    }
                    else if (PartitionKey != Id)
                    {
                        throw new Exception($"Someone has intentionally set the PartitionKey {PartitionKey} to something other than the Id {Id} of this object, which in this case is invalid.");
                    }
                    break;
                case JsonEntityPartitionKeyBaseAttribute.PartitionKeySchemeEnum.RelatedObjectId:
                    JsonEntityIdServices.ThrowIfInvalid(pka.RelatedType, PartitionKey);
                    break;
            }
        }
    }

    public void PreSave(IJsonEntityContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);
        OnPreSave(container);
    }

    protected virtual void OnPreSave(IJsonEntityContainer container)
    {
        if (container is ITenantIdHolder containerTenantIdHolder && this is ITenantIdHolder myTenantIdHolder)
        {
            CrossTenantException.ThrowIfCrossTenant(containerTenantIdHolder, myTenantIdHolder);
            myTenantIdHolder.TenantId = containerTenantIdHolder.TenantId;
        }
    }


    #region IValidate

    public void Validate()
        => OnValidate();

    protected virtual void OnValidate()
        => ExceptionHelpers.AggregateExceptionsAndReThrow(
            () => JsonEntityIdServices.ThrowIfInvalid(GetType(), Id),
            () => Requires.Text(DataType),
            () => Requires.Text(PartitionKey),
            () =>
            {
                foreach (var pi in GetType().GetPropertiesPublicInstanceReadWrite())
                {
                    var reqAttr = pi.GetCustomAttribute<RequiredAttribute>();
                    if (reqAttr != null)
                    {
                        var val = pi.GetValue(this);
                        if (val == null)
                        {
                            throw new ValidationException($"Property [{pi.Name}] is required.");
                        }
                        if (val is string sVal && string.IsNullOrWhiteSpace(sVal))
                        {
                            throw new ValidationException($"Property [{pi.Name}] is required.");
                        }
                    }
                }
            },
            () =>
            {
                foreach (var pi in GetType().GetPropertiesPublicInstanceReadWrite())
                {
                    var fkAttr = pi.GetCustomAttribute<JsonEntityRelatedKeyBaseAttribute>();
                    if (fkAttr?.Relation == JsonEntityRelatedKeyBaseAttribute.RelationEnum.ForeignKey)
                    {
                        var sVal = pi.GetValue(this) as string;
                        if (sVal == null || string.IsNullOrWhiteSpace(sVal))
                        {
                            var reqAttr = pi.GetCustomAttribute<RequiredAttribute>();
                            if (reqAttr != null)
                            {
                                throw new ValidationException($"Property [{pi.Name}] is a required foreignKey to {fkAttr.RelatedType.Name}.");
                            }
                        }
                        if (sVal is string s && string.IsNullOrWhiteSpace(s))
                        {
                            JsonEntityIdServices.ThrowIfInvalid(fkAttr.RelatedType, sVal);
                        }
                    }
                }
            }
        );

    #endregion

    #region Deserialization

    protected virtual void OnDeserialized()
    { }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        OnDeserialized();
        PostDeserializeHash = ComputePostDeserializeHash ? CreateHash() : null;
    }

    private static readonly NonCryptographicHashAlgorithm HashAlg = new XxHash64(Stuff.Random.NextInt64());

    protected virtual bool ComputePostDeserializeHash => true;

    private string? PostDeserializeHash;

    private string CreateHash()
    {
        var json = ToJson();
        var buf = System.Text.UTF8Encoding.UTF8.GetBytes(json);
        byte[] hash;
        lock (HashAlg)
        {
            HashAlg.Reset();
            HashAlg.Append(buf);
            hash = HashAlg.GetHashAndReset();
        }
        return System.Convert.ToBase64String(hash);
    }

    public bool HasChangedSinceDeserialization()
        => CreateHash() != PostDeserializeHash;

    #endregion

    #region IPrimaryKey

    string IPrimaryKey<string>.Key
        => Id;

    object IPrimaryKey.Key
        => Id;

    #endregion
}
