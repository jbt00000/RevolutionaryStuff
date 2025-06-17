using RevolutionaryStuff.Crm.Implementation;

namespace RevolutionaryStuff.Crm;

public class CrmPersonContact : CrmContact, ICrmPersonContact
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public DateOnly? BirthDate { get; set; }
}
