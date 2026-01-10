using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace RevolutionaryStuff.ApiCore;

public static class HttpHelpers
{
    public static string? GetSingleStringValueFromHeaderOrQueryString(this HttpContext httpContext, string headerName, string? queryStringName = null)
        => httpContext.Request.Headers[headerName].SingleOrDefault()
           ?? httpContext.Request.Query[queryStringName ?? headerName].SingleOrDefault();

    public static void Throw404IfNull(object? o)
    {
        if (o == null)
            throw new HttpRequestException(null, null, System.Net.HttpStatusCode.NotFound);
    }

    public static IHeaderDictionary SetContentDisposition(this IHeaderDictionary headers, string fileName, bool asAttachment = true)
    {
        headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue(asAttachment ? "attachment" : "inline")
        {
            FileName = $"\"{fileName}\""
        }.ToString();
        return headers;
    }

    public static Task SendStreamAsync(this HttpResponse resp, Stream st, string fileName, string contentType, bool asAttachment = true)
    {
        Requires.ReadableStreamArg(st);
        if (contentType != null)
        {
            resp.ContentType = contentType;
        }
        if (fileName != null)
        {
            resp.Headers.SetContentDisposition(fileName, asAttachment);
        }
        return st.CopyToAsync(resp.Body);
    }
}
