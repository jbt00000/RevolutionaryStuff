using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.ApplicationParts.Services
{
    public class HttpClientHttpMessageSender : BaseDisposable, IHttpMessageSender
    {
        public HttpClientHttpMessageSender(HttpClient httpClient)
        {
            Requires.NonNull(httpClient, nameof(httpClient));
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
}
