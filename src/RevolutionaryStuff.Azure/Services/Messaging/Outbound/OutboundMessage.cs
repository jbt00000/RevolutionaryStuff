using System.IO;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Azure.Services.Messaging.Outbound;

public class OutboundMessage : BaseDisposable, IValidate
{
    public string ContentType { get; set; }

    internal Stream Payload { get; set; }

    internal long Size
        => Payload?.Length ?? 0;

    public Dictionary<string, object> Properties { get; private set; }

    public OutboundMessage() { }

    public static OutboundMessage Create(Stream stream, string contentType)
    {
        Requires.ReadableStreamArg(stream);
        if (stream.CanSeek)
        {
            Requires.True(stream.Position == 0, "stream.Position");
        }
        return new()
        {
            Payload = stream,
            ContentType = contentType
        };
    }

    public static OutboundMessage Create(IJsonSerializable js)
        => Create(js.ToJson(), MimeType.Application.Json.PrimaryContentType);

    public static OutboundMessage Create(string text, string contentType)
        => new()
        {
            Payload = new MemoryStream(text.ToUTF8()),
            ContentType = contentType
        };

    void IValidate.Validate()
        => ExceptionHelpers.AggregateExceptionsAndReThrow(
            () => Requires.Text(ContentType),
            () => ArgumentNullException.ThrowIfNull(Payload)
            );

    public void SetProperty(string name, object val)
        => (Properties ??= [])[name] = val;
}
