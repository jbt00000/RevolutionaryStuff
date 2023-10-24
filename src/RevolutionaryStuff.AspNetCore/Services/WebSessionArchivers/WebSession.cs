using System.IO;
using Microsoft.AspNetCore.Http;

namespace RevolutionaryStuff.AspNetCore.Services.SazGenerators;

public class WebSession
{
    public HttpRequest Request { get; init; }
    public Stream RequestBody { get; set; }
    //public HttpResponse Response { get; set; }

    public WebSession(HttpRequest request, Stream requestBody = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        Request = request;
        RequestBody = requestBody;
    }
}
