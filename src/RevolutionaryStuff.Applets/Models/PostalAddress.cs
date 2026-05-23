using System.Text.Json.Serialization;

namespace RevolutionaryStuff.Applets.Models;

/// <summary>
/// Represents a postal address.
/// </summary>
public class PostalAddress : IPostalAddress
{
    private IPostalAddress I => this;

    /// <summary>
    /// Gets or sets the primary address line (e.g., street address).
    /// </summary>
    [JsonPropertyName("addressLine1")]
    public string AddressLine1 { get; set; }

    /// <summary>
    /// Gets or sets the secondary address line (e.g., apartment, suite number).
    /// </summary>
    [JsonPropertyName("addressLine2")]
    public string AddressLine2 { get; set; }

    /// <summary>
    /// Gets or sets the city or locality.
    /// </summary>
    [JsonPropertyName("city")]
    public string City { get; set; }

    /// <summary>
    /// Gets or sets the state, province, or region.
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; }

    /// <summary>
    /// Gets or sets the postal or ZIP code.
    /// </summary>
    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    [JsonPropertyName("country")]
    public string Country { get; set; }

    /// <summary>
    /// Gets or sets the free form address, addresses which may or may not be cleanly parsed, presented partially, or non-standard.
    /// </summary>
    [JsonPropertyName("freeForm")]
    public string FreeForm { get; set; }

    public static PostalAddress Create(IPostalAddress other)
        => other == null ? null : Create(other.AddressLine1, other.AddressLine2, other.City, other.State, other.PostalCode, other.Country);

    public static PostalAddress Create(string addressLine1, string addressLine2, string city, string state, string postalCode, string country)
        => new()
        {
            AddressLine1 = StringHelpers.TrimOrNull(addressLine1),
            AddressLine2 = StringHelpers.TrimOrNull(addressLine2),
            City = StringHelpers.TrimOrNull(city),
            State = StringHelpers.TrimOrNull(state),
            PostalCode = StringHelpers.TrimOrNull(postalCode),
            Country = StringHelpers.TrimOrNull(country),
        };

    public override string ToString()
        => FreeForm ?? I.CreateFreeform();

    [JsonIgnore]
    public bool IsEmpty
        =>
        string.IsNullOrEmpty(AddressLine1) &&
        string.IsNullOrEmpty(AddressLine2) &&
        string.IsNullOrEmpty(City) &&
        string.IsNullOrEmpty(State) &&
        string.IsNullOrEmpty(PostalCode) &&
        string.IsNullOrEmpty(Country) &&
        string.IsNullOrEmpty(FreeForm);

    [JsonIgnore]
    public PostalAddress NullIfEmpty
        => IsEmpty ? null : this;
}
