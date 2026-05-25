using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace RevolutionaryStuff.ApiCore.Json;

/// <summary>
/// A <see cref="IJsonTypeInfoResolver"/> that injects <see cref="EnumMemberJsonConverter{T}"/> for every
/// enum-typed property so that <see cref="System.Runtime.Serialization.EnumMemberAttribute"/> values are
/// always honoured on the wire. Insert at position 0 in
/// <see cref="JsonSerializerOptions.TypeInfoResolverChain"/> to beat the default resolver, which in
/// .NET 9+ takes priority over converters registered in <see cref="JsonSerializerOptions.Converters"/>.
/// </summary>
public sealed class EnumMemberJsonTypeInfoResolver : IJsonTypeInfoResolver
{
    private static readonly DefaultJsonTypeInfoResolver Inner = new();

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = Inner.GetTypeInfo(type, options);
        if (typeInfo is null) return null;

        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (var prop in typeInfo.Properties)
            {
                if (IsEnumOrNullableEnum(prop.PropertyType, out var enumType))
                {
                    prop.CustomConverter = MakeConverter(prop.PropertyType, enumType!);
                }
            }
        }

        return typeInfo;
    }

    private static bool IsEnumOrNullableEnum(Type type, out Type? enumType)
    {
        if (type.IsEnum)
        {
            enumType = type;
            return true;
        }
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying?.IsEnum == true)
        {
            enumType = underlying;
            return true;
        }
        enumType = null;
        return false;
    }

    private static JsonConverter MakeConverter(Type propertyType, Type enumType)
    {
        var converterType = (Nullable.GetUnderlyingType(propertyType) == null ? typeof(EnumMemberJsonConverter<>) : typeof(NullableEnumMemberJsonConverter<>)).MakeGenericType(enumType);
        return (JsonConverter)Activator.CreateInstance(converterType, new object[] { true })!;
    }
}
