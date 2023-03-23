using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Core.Services.Correlation;

public class HttpContextCorrelationIdFinder : ICorrelationIdFinder
{
    public const string DefaultCorrelationIdHeaderKey = "x-correlation-id";


    private readonly IHttpContextAccessor HttpContextAccessor;
    private readonly IOptions<Config> ConfigOptions;

    public class Config
    {
        public const string ConfigSectionName = Stuff.ConfigSectionNamePrefix + "HttpContextCorrelationIdFinderConfig";

        public IList<string> HttpHeaderNames { get; set; } = new[] { DefaultCorrelationIdHeaderKey };
    }

    public HttpContextCorrelationIdFinder(IOptions<Config> configOptions, IHttpContextAccessor httpContextAccessor=null)
    {
        //ArgumentNullException.ThrowIfNull(httpContextAccessor);
        ArgumentNullException.ThrowIfNull(configOptions);

        HttpContextAccessor = httpContextAccessor;
        ConfigOptions = configOptions;
    }

    IList<string> ICorrelationIdFinder.CorrelationIds
    {
        get
        {
            var req = HttpContextAccessor?.HttpContext?.Request;
            if (req == null) return null;
            List<string> ret = null;
            foreach (var headerName in ConfigOptions.Value.HttpHeaderNames.NullSafeEnumerable().Select(z=>z.TrimOrNull()).WhereNotNull())
            {
                var vals = req.Headers[headerName];
                if (vals.Count > 0)
                {
                    ret ??= new();
                    ret.AddRange(vals);
                }
            }
            return ret.Count < 2 ? ret : ret.Distinct().ToList();
        }
    }
}
