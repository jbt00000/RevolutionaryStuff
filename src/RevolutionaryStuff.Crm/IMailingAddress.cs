namespace RevolutionaryStuff.Crm;

public interface IMailingAddress
{
    /// <summary>
    /// Gets the primary address line (e.g., street address).
    /// </summary>
    string? AddressLine1 { get; }

    /// <summary>
    /// Gets the secondary address line (e.g., apartment, suite number).
    /// </summary>
    string? AddressLine2 { get; }

    /// <summary>
    /// Gets the city or locality.
    /// </summary>
    string? City { get; }

    /// <summary>
    /// Gets the state, province, or region.
    /// </summary>
    string? State { get; }

    /// <summary>
    /// Gets the postal or ZIP code.
    /// </summary>
    string? PostalCode { get; }

    /// <summary>
    /// ISO-3166-1 Alpha-3 country code.
    /// </summary>
    string? Country { get; }
}
