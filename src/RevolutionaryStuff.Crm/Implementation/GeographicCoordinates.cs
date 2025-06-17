namespace RevolutionaryStuff.Crm.Implementation;

public record GeographicCoordinates(double? Longitude, double? Latitude) : IGeographicCoordinates
{
    public override string ToString()
        => $"({Latitude}, {Longitude})";
}
