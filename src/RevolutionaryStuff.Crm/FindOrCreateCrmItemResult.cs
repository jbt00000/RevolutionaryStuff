namespace RevolutionaryStuff.Crm;

public record FindOrCreateCrmItemResult : CrmItemResult
{
    public FindOrCreateCrmItemResult()
    { }

    public FindOrCreateCrmItemResult(string? id, bool? success = null)
        : base(id, success)
    { }

    public static FindOrCreateCrmItemResult CreateFromResult(FindCrmItemResult result)
        => new(result.Id, result.Success);

    public static FindOrCreateCrmItemResult CreateFromResult(CreateCrmItemResult result)
        => new(result.Id, result.Success);
}
