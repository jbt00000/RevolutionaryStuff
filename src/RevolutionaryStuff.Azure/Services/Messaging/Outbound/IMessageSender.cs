using System.Threading;

namespace RevolutionaryStuff.Azure.Services.Messaging.Outbound;

public interface IMessageSender
{
    Task SendAsync(string port, OutboundMessage message, Action<IDictionary<string, object>> propertyOverride = null, MessageSendSettings settings = null, CancellationToken cancellationToken = default);

    #region default Implementation

    Task SendJsonAsync(string port, string json, Action<IDictionary<string, object>> propertyOverride = null, MessageSendSettings settings = null, CancellationToken cancellationToken = default)
        => SendAsync(port, OutboundMessage.Create(json, MimeType.Application.Json.PrimaryContentType), propertyOverride, settings, cancellationToken);

    #endregion
}
