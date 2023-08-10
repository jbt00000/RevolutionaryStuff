namespace RevolutionaryStuff.Azure.Services.Messaging.Outbound;

public class MessageSendSettings
{
    public Dictionary<string, string> Properties { get; set; }
    public TimeSpan? SendIn { get; set; }
    public DateTimeOffset? SendAt { get; set; }
}
