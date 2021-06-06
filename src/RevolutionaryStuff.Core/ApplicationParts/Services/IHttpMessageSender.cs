using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Core.ApplicationParts.Services
{
    public interface IHttpMessageSender
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken=default);
    }
}
