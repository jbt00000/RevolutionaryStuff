namespace RevolutionaryStuff.Crm;

public record FindCrmItemResult : CrmItemResult
{
    public FindCrmItemResult() { }

    public FindCrmItemResult(string? id, bool? success = null)
        : base(id, success) { }
}
