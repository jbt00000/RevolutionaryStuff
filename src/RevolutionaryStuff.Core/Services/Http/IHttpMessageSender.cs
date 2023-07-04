using System.Net.Http;
using System.Threading;

namespace RevolutionaryStuff.Core.Services.Http;

public interface IHttpMessageSender
{
    const string DefaultCorrelationIdHeaderKey = "x-correlation-id";

    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}

