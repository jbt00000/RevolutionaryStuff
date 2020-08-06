using Newtonsoft.Json;
using RevolutionaryStuff.Core;

namespace Microsoft.AspNetCore.Mvc
{
    public class NewtonsoftJsonResult : ContentResult
    {
        public NewtonsoftJsonResult(object o)
        {
            Content = JsonConvert.SerializeObject(o, Formatting.Indented);
            ContentType = MimeType.Application.Json.PrimaryContentType;
        }
    }
}
