using System.Runtime.Serialization;

namespace RevolutionaryStuff.Data.Cosmos.Services.Setup;

public enum TriggerTypeEnum
{
    /// <summary>
    /// Trigger should be executed before the associated operation(s).
    /// </summary>
    [EnumMember(Value = "pre")]
    Pre,

    /// <summary>
    /// Trigger should be executed after the associated operation(s).
    /// </summary>
    [EnumMember(Value = "post")]
    Post
}
