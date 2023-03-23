using System.Net.Http;
using System.Threading;

namespace RevolutionaryStuff.Core.Services.Http;

public interface IHttpMessageSender
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}

