namespace RevolutionaryStuff.Storage;

public class ExternalAccessSettings
{
    public static ExternalAccessSettings CreateDefaultSettings()
        => new()
        {
            AccessType = AccessTypeEnum.Read,
            ExpiresIn = TimeSpan.FromHours(1)
        };

    private static readonly TimeSpan FallbackExpires = TimeSpan.FromHours(1);
    public DateTimeOffset CalculateExpiresAt(TimeSpan? fallbackExpires = null)
    {
        var expires = ExpiresAt != null
            ? ExpiresAt.Value
            : ExpiresIn != null
                ? DateTimeOffset.UtcNow.Add(ExpiresIn.Value)
                : DateTimeOffset.UtcNow.Add(fallbackExpires ?? FallbackExpires);
        return expires;
    }

    public enum AccessTypeEnum { Read }
    public AccessTypeEnum AccessType { get; set; } = AccessTypeEnum.Read;
    public TimeSpan? ExpiresIn { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string OverrideContentType { get; set; }
    public bool SetContentTypeBasedOnFileExtension { get; set; }
}
