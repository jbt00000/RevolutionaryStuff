namespace RevolutionaryStuff.Crm;

public interface ICrmContact
{
    string? Name { get; }
    string? Phone { get; }
    string? Email { get; }
    IMailingAddress? Address { get; }
    IDictionary<string, object?>? AdditionalFields { get; }

    IDictionary<string, object?>? ToDictionary()
    {
        Dictionary<string, object?> d = new()
        {
            { nameof(Name), Name },
            { nameof(Phone), Phone },
            { nameof(Email), Email },
            { nameof(Address), Address }
        };
        if (AdditionalFields != null)
        {
            foreach (var kvp in AdditionalFields)
            {
                d[kvp.Key] = kvp.Value;
            }
        }
        return d;
    }
}
