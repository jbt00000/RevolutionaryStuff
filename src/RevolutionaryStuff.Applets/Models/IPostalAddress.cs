namespace RevolutionaryStuff.Applets.Models;

public interface IPostalAddress
{
    /// <summary>
    /// Gets the primary address line (e.g., street address).
    /// </summary>
    string AddressLine1 { get; }

    /// <summary>
    /// Gets the secondary address line (e.g., apartment, suite number).
    /// </summary>
    string AddressLine2 { get; }

    /// <summary>
    /// Gets the city or locality.
    /// </summary>
    string City { get; }

    /// <summary>
    /// Gets the state, province, or region.
    /// </summary>
    string State { get; }

    /// <summary>
    /// Gets the postal or ZIP code.
    /// </summary>
    string PostalCode { get; }

    /// <summary>
    /// Gets the country.
    /// </summary>
    string Country { get; }
}
