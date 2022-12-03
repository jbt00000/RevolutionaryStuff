using System.Net.Http;
using System.Threading;

namespace RevolutionaryStuff.Core.ApplicationParts.Services;

internal class HttpClientHttpMessageSender : BaseDisposable, IHttpMessageSender
{
    public HttpClientHttpMessageSender(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; }

    protected override void OnDispose(bool disposing)
    {
        base.OnDispose(disposing);
        HttpClient.Dispose();
    }

    Task<HttpResponseMessage> IHttpMessageSender.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => HttpClient.SendAsync(request, cancellationToken);
}
