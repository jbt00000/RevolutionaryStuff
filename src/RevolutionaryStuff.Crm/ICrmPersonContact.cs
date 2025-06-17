namespace RevolutionaryStuff.Crm;

public interface ICrmPersonContact : ICrmContact
{
    DateOnly? BirthDate { get; set; }
    string? FirstName { get; set; }
    string? LastName { get; set; }
    string? MiddleName { get; set; }
}
