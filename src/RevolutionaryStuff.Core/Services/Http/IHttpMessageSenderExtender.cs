using System.Net.Http;

namespace RevolutionaryStuff.Core.Services.Http;

public interface IHttpMessageSenderExtender
{
    void ModifyMessage(HttpRequestMessage request);
}
