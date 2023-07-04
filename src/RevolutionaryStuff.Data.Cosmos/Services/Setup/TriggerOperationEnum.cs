using System.Runtime.Serialization;

namespace RevolutionaryStuff.Data.Cosmos.Services.Setup;

/// <summary>
/// Specifies the operations on which a trigger should be executed in the Azure Cosmos DB service.
/// </summary> 
public enum TriggerOperationEnum
{
    /// <summary>
    /// Specifies create operations only.
    /// </summary>
    [EnumMember(Value = "create")]
    Create,

    /// <summary>
    /// Specifies update operations only.
    /// </summary>
    [EnumMember(Value = "update")]
    Update,

    /// <summary>
    /// Specifies delete operations only.
    /// </summary>
    [EnumMember(Value = "delete")]
    Delete,

    /// <summary>
    /// Specifies replace operations only.
    /// </summary>
    [EnumMember(Value = "replace")]
    Replace
}
