namespace RevolutionaryStuff.Crm;

public abstract record CrmItemResult : ICrmItemResult
{
    public string? Id { get; init; }

    public bool Success { get; init; }

    public static implicit operator string?(CrmItemResult result)
        => result?.Id;

    protected CrmItemResult()
    { }

    protected CrmItemResult(string? id, bool? success = null)
    {
        Id = id;
        Success = success ?? id != null;
    }
}
