namespace RevolutionaryStuff.Crm;

public record FindOrCreateCrmItemSettings
{
    public FindCrmItemSettings? FindCrmItemSettings { get; init; }
    public CreateCrmItemSettings? CreateCrmItemSettings { get; init; }
}
