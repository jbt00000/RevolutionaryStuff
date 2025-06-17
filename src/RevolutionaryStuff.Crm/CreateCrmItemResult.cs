namespace RevolutionaryStuff.Crm;

public record CreateCrmItemResult : CrmItemResult
{
    public CreateCrmItemResult() { }

    public CreateCrmItemResult(string id, bool? success = null)
        : base(id, success) { }
}
