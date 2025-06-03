using System.IO;
using System.Net.Http;
using System.Threading;

namespace RevolutionaryStuff.Core.Services.Http;

public interface IHttpMessageSender
{
    const string DefaultCorrelationIdHeaderKey = "x-correlation-id";

    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);

    #region Helpers

    Task<HttpResponseMessage> GetAsync(Uri u)
        => SendAsync(new(HttpMethod.Get, u));

    async Task<string> GetStringAsync(Uri u, bool throwOnNonSuccess = true)
    {
        var resp = await GetAsync(u);
        if (throwOnNonSuccess)
            resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    async Task<Stream> GetStreamAsync(Uri u, bool throwOnNonSuccess = true)
    {
        var resp = await GetAsync(u);
        if (throwOnNonSuccess)
            resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStreamAsync();
    }

    async Task<GetDataResult> GetDataAsync(Uri u, bool throwOnNonSuccess = true)
    {
        var resp = await GetAsync(u);
        if (throwOnNonSuccess)
            resp.EnsureSuccessStatusCode();
        return new(await resp.Content.ReadAsStreamAsync(), resp);
    }

    record GetDataResult(Stream Stream, HttpResponseMessage Message)
    {
        public string ContentType => Message.Content.Headers.ContentType?.ToString() ?? MimeType.Application.OctetStream.PrimaryContentType;
        public long? ContentLength => Message.Content.Headers.ContentLength;
    }

    #endregion
}
