namespace RevolutionaryStuff.Crm.Implementation;

public class CrmContact : ICrmContact
{
    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public MailingAddress? Address { get; set; }

    IMailingAddress? ICrmContact.Address => Address;

    IDictionary<string, object?>? ICrmContact.AdditionalFields => AdditionalFields;

    public Dictionary<string, object?>? AdditionalFields { get; set; }

    public override string ToString()
        => Name ?? base.ToString()!;
}
