using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace RevolutionaryStuff.ApiCore;

public static class HttpHelpers
{
    extension(HttpContext source)
    {
        public string? GetSingleStringValueFromHeaderOrQueryString(string headerName, string? queryStringName = null)
                => source.Request.Headers[headerName].SingleOrDefault()
                   ?? source.Request.Query[queryStringName ?? headerName].SingleOrDefault();
    }

    public static void Throw404IfNull(object? o)
    {
        if (o == null)
            throw new HttpRequestException(null, null, System.Net.HttpStatusCode.NotFound);
    }

    extension(IHeaderDictionary sources)
    {
        public IHeaderDictionary SetContentDisposition(string fileName, bool asAttachment = true)
        {
            sources.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue(asAttachment ? "attachment" : "inline")
            {
                FileName = $"\"{fileName}\""
            }.ToString();
            return sources;
        }
    }

    extension(HttpResponse source)
    {
        public Task SendStreamAsync(Stream st, string fileName, string contentType, bool asAttachment = true)
        {
            Requires.ReadableStreamArg(st);
            if (contentType != null)
            {
                source.ContentType = contentType;
            }
            if (fileName != null)
            {
                source.Headers.SetContentDisposition(fileName, asAttachment);
            }
            return st.CopyToAsync(source.Body);
        }
    }
}
