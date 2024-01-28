using System.Linq.Expressions;

namespace RevolutionaryStuff.Data.JsonStore.Store;

public record PatchOperation
{
    public required PatchOperationTypeEnum PatchOperationType { get; init; }
    public required string Path { get; init; }
    public object? Value { get; init; }

    public static PatchOperation Add(string path, object value)
        => new() { Path = path, Value = value, PatchOperationType = PatchOperationTypeEnum.Add };

    public static PatchOperation Replace(string path, object value)
        => new() { Path = path, Value = value, PatchOperationType = PatchOperationTypeEnum.Replace };

    private const string PathSeparator = "/";

    public static PatchOperation Create<TEntity>(Expression<Func<TEntity, object>> property, object updatedValue, PatchOperationTypeEnum op = PatchOperationTypeEnum.Add)
    {
        var path = $"{PathSeparator}{property.GetFullyQualifiedName(JsonHelpers.GetJsonPropertyName, PathSeparator)}";
        var po = op switch
        {
            PatchOperationTypeEnum.Add => PatchOperation.Add(path, updatedValue),
            PatchOperationTypeEnum.Replace => PatchOperation.Replace(path, updatedValue),
            _ => throw new UnexpectedSwitchValueException(op)
        };
        return po;
    }
}

public enum PatchOperationTypeEnum
{
    Add,
    Replace
}
