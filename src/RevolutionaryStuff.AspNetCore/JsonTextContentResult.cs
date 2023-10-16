using RevolutionaryStuff.Data.JsonStore.Serialization.Json;

namespace Microsoft.AspNetCore.Mvc;

public class JsonTextContentResult : ContentResult
{
    public JsonTextContentResult(object o, IJsonSerializer serializer = null)
        : this(o, null, serializer)
    { }

    public JsonTextContentResult(object o, System.Net.HttpStatusCode? statusCode = null, IJsonSerializer serializer = null)
    {
        var json = (serializer ?? IJsonSerializer.Default).ToJson(o);
        Content = json;
        ContentType = MimeType.Application.Json.PrimaryContentType;
        if (statusCode != null)
        {
            StatusCode = (int)statusCode.Value;
        }
    }
}
