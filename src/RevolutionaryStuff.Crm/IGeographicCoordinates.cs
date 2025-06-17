namespace RevolutionaryStuff.Crm;

public interface IGeographicCoordinates
{
    /// <summary>
    /// Gets the longitude coordinate in decimal degrees.
    /// </summary>
    /// <remarks>
    /// Valid values range from -180 to 180 degrees, with positive values representing east
    /// and negative values representing west of the Prime Meridian.
    /// </remarks>
    double? Longitude { get; }

    /// <summary>
    /// Gets the latitude coordinate in decimal degrees.
    /// </summary>
    /// <remarks>
    /// Valid values range from -90 to 90 degrees, with positive values representing north
    /// and negative values representing south of the Equator.
    /// </remarks>
    double? Latitude { get; }
}
