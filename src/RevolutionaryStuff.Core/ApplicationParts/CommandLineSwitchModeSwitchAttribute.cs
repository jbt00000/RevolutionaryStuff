namespace RevolutionaryStuff.Core.ApplicationParts;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class CommandLineSwitchModeSwitchAttribute : CommandLineSwitchAttribute
{
    public CommandLineSwitchModeSwitchAttribute(string name, string description = null)
        : base(name, true, description)
    { }
}

