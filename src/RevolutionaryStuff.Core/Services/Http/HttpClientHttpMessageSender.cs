using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.Services.Correlation;

namespace RevolutionaryStuff.Core.Services.Http;

internal class HttpClientHttpMessageSender : BaseLoggingDisposable, IHttpMessageSender
{
    public class Config
    {
        public const string ConfigSectionName = Stuff.ConfigSectionNamePrefix + "HttpClientHttpMessageSenderConfig";
        public string CorrelationIdHeaderKey { get; set; } = IHttpMessageSender.DefaultCorrelationIdHeaderKey;
        public IDictionary<string, string> HeaderValueByHeaderName { get; set; }
        public string UserAgentString { get; set; }
    }

    private readonly IServiceProvider ServiceProvider;
    private readonly IHttpClientFactory HttpClientFactory;
    private readonly ICorrectionIdFindOrCreate CorrectionIdFindOrCreate;
    private readonly IOptions<Config> ConfigOptions;

    public HttpClientHttpMessageSender(ICorrectionIdFindOrCreate correctionIdFindOrCreate, IServiceProvider serviceProvider, IOptions<Config> configOptions, ILogger<HttpClientHttpMessageSender> logger)
        : this(null, correctionIdFindOrCreate, serviceProvider, configOptions, logger)
    { }

    public HttpClientHttpMessageSender(IHttpClientFactory httpClientFactory, ICorrectionIdFindOrCreate correctionIdFindOrCreate, IServiceProvider serviceProvider, IOptions<Config> configOptions, ILogger<HttpClientHttpMessageSender> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(correctionIdFindOrCreate);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(configOptions);

        HttpClientFactory = httpClientFactory;
        CorrectionIdFindOrCreate = correctionIdFindOrCreate;
        ServiceProvider = serviceProvider;
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
        SetHeaders(request);
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    private IList<IHttpMessageSenderExtender> ExtendersField;

    private IList<IHttpMessageSenderExtender> Extenders
        => ExtendersField ??= ServiceProvider.GetServices<IHttpMessageSenderExtender>().ToList();

    private void SetHeaders(HttpRequestMessage request)
    {
        SetConfigHeaders(request);
        SetCorrelationIdHeader(request);
        Extenders.ForEach(z => z.ModifyMessage(request));
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
