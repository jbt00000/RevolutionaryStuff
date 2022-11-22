using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RevolutionaryStuff.AspNetCore;

public class NewtonsoftJsonResult : ContentResult
{
    public static JsonSerializerSettings DefaultSerializerSettings = JsonNetHelpers.DefaultSerializerSettings;

    public NewtonsoftJsonResult(object o, JsonSerializer serializer, JsonSerializerSettings settings = null)
        : this(o, null, serializer, settings)
    { }

    public NewtonsoftJsonResult(object o, System.Net.HttpStatusCode? statusCode = null, JsonSerializer serializer = null, JsonSerializerSettings settings = null)
    {
        if (serializer == null)
        {
            settings ??= DefaultSerializerSettings;
            Content = JsonConvert.SerializeObject(o, Formatting.Indented, settings);
        }
        else
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                serializer.Serialize(sw, o);
            }
            Content = sb.ToString();
        }
        ContentType = MimeType.Application.Json.PrimaryContentType;
        if (statusCode != null)
            StatusCode = (int)statusCode.Value;
    }
}
