using System.IO;
using System.Text;
using Newtonsoft.Json;
using RevolutionaryStuff.Core;

namespace Microsoft.AspNetCore.Mvc
{
    public class NewtonsoftJsonResult : ContentResult
    {
        public NewtonsoftJsonResult(object o, JsonSerializer serializer = null)
        {
            if (serializer == null)
            {
                base.Content = JsonConvert.SerializeObject(o, Formatting.Indented);
            }
            else
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    serializer.Serialize(sw, o);
                }
                base.Content = sb.ToString();
            }
            base.ContentType = MimeType.Application.Json.PrimaryContentType;
        }
    }
}
