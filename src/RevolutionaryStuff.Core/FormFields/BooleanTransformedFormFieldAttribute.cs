namespace RevolutionaryStuff.Core.FormFields;

[AttributeUsage(AttributeTargets.Property)]
public class BooleanTransformedFormFieldAttribute : TransformedFormFieldAttribute
{
    public readonly string TrueVal;
    public readonly string FalseVal;
    public readonly string OtherVal;

    public BooleanTransformedFormFieldAttribute(string trueVal, string falseVal, string otherVal = null)
    {
        TrueVal = trueVal;
        FalseVal = falseVal;
        OtherVal = otherVal;
    }

    public override object Transform(object val)
    {
        return val is bool ? val.Equals(true) ? TrueVal : FalseVal : (object)OtherVal;
    }
}
