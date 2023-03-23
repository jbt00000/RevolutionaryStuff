using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.Services.Correlation;

namespace RevolutionaryStuff.Core.Services.Http;

internal class HttpClientHttpMessageSender : BaseLoggingDisposable, IHttpMessageSender
{
    public class Config
    {
        public const string ConfigSectionName = Stuff.ConfigSectionNamePrefix + "HttpClientHttpMessageSenderConfig";
        public string CorrelationIdHeaderKey { get; set; } = HttpContextCorrelationIdFinder.DefaultCorrelationIdHeaderKey;
        public IDictionary<string, string> HeaderValueByHeaderName { get; set; }
        public string UserAgentString { get; set; }
    }

    private readonly IHttpClientFactory HttpClientFactory;
    private readonly ICorrectionIdFindOrCreate CorrectionIdFindOrCreate;
    private readonly IOptions<Config> ConfigOptions;

    public HttpClientHttpMessageSender(ICorrectionIdFindOrCreate correctionIdFindOrCreate, IOptions<Config> configOptions, ILogger<HttpClientHttpMessageSender> logger)
        : this(null, correctionIdFindOrCreate, configOptions, logger)
    { }

    public HttpClientHttpMessageSender(IHttpClientFactory httpClientFactory, ICorrectionIdFindOrCreate correctionIdFindOrCreate, IOptions<Config> configOptions, ILogger<HttpClientHttpMessageSender> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(correctionIdFindOrCreate);
        ArgumentNullException.ThrowIfNull(configOptions);

        HttpClientFactory = httpClientFactory;
        CorrectionIdFindOrCreate = correctionIdFindOrCreate;
        ConfigOptions = configOptions;
    }

    protected override void OnDispose(bool disposing)
    {
        base.OnDispose(disposing);
        HttpClientField?.Dispose();
    }

    private HttpClient HttpClientField;
    private HttpClient HttpClient
        => HttpClientField ??= HttpClientFactory?.CreateClient() ?? new HttpClient();

    async Task<HttpResponseMessage> IHttpMessageSender.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await SetHeadersAsync(request);
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    private async Task SetHeadersAsync(HttpRequestMessage request)
    {
        SetConfigHeaders(request);
        SetCorrelationIdHeader(request);
    }

    private void SetCorrelationIdHeader(HttpRequestMessage request)
    {
        var config = ConfigOptions.Value;
        if (!request.Headers.Contains(config.CorrelationIdHeaderKey))
        {
            request.Headers.Add(config.CorrelationIdHeaderKey, CorrectionIdFindOrCreate.CorrelationId);
        }
    }

    private void SetConfigHeaders(HttpRequestMessage request)
    {
        var config = ConfigOptions.Value;
        if (config.HeaderValueByHeaderName != null)
        {
            foreach (var kvp in config.HeaderValueByHeaderName)
            {
                request.Headers.Add(kvp.Key, kvp.Value);
            }
        }

        if (config.UserAgentString != null)
        {
            request.Headers.UserAgent.Add(ProductInfoHeaderValue.Parse(config.UserAgentString));
        }
    }
}
