namespace RevolutionaryStuff.Core.ApplicationParts;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CommandLineMandateAttribute : Attribute
{
    public int[] EnumVals;
    public string[] MandatoryKeys;
    public string[] OptionalKeys;

    public CommandLineMandateAttribute(int[] enumVals, string[] mandatoryKeys, string[] optionalKeys = null)
    {
        EnumVals = enumVals;
        MandatoryKeys = mandatoryKeys;
        OptionalKeys = optionalKeys;
    }

    public CommandLineMandateAttribute(int enumVal, string[] keys, string[] optionalKeys = null)
        : this([enumVal], keys, optionalKeys)
    { }
}
