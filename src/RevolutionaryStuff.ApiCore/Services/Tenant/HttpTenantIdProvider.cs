using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.Tenant;

namespace RevolutionaryStuff.ApiCore.Services.Tenant;

internal class HttpTenantIdProvider(IOptions<HttpTenantIdProvider.Config> ConfigOptions, IHttpContextAccessor ContextAccessor, ILogger<HttpTenantIdProvider> logger)
    : BaseLoggingDisposable(logger), IHttpTenantIdProvider
{
    public class Config : IValidate
    {
        public const string ConfigSectionName = "HttpTenantIdProvider";

        public string? FallbackTenantId { get; set; }

        public List<SourceClass> Sources { get; set; } = [
            new SourceClass(){ KeyName= "x-tenantId", SourceLocation = SourceLocationEnum.HttpHeaders },
            new SourceClass(){ KeyName= "x-tenantId", SourceLocation = SourceLocationEnum.HttpQueryString },
            new SourceClass(){ KeyName= "tid", SourceLocation = SourceLocationEnum.HttpQueryString }
            ];

        public class SourceClass : IValidate
        {
            public bool Disabled { get; set; }
            public string? KeyName { get; set; }

            public SourceLocationEnum SourceLocation { get; set; }

            public bool IsUsable => !Disabled && !string.IsNullOrWhiteSpace(KeyName);

            void IValidate.Validate()
            {
                if (Disabled) return;
                Requires.Text(KeyName);
            }
        }

        public enum SourceLocationEnum
        {
            HttpHeaders,
            HttpQueryString
        }

        void IValidate.Validate()
            => Sources.NullSafeEnumerable().ForEach(z => Requires.Valid(z));
    }

    private string? TenantId;

    private bool TenantIdFetched;

    Task<string?> ITenantIdProvider.GetTenantIdAsync()
    {
        if (!TenantIdFetched)
        {
            var config = ConfigOptions.Value;
            var context = ContextAccessor.HttpContext;
            string? val = null;
            if (context != null)
            {
                foreach (var s in config.Sources.NullSafeEnumerable().Where(z => z.IsUsable))
                {
                    switch (s.SourceLocation)
                    {
                        case Config.SourceLocationEnum.HttpHeaders:
                            try
                            {
                                val = context.Request.Headers[s.KeyName!].SingleOrDefault().TrimOrNull();
                                goto SetTheTenantId;
                            }
                            catch (InvalidOperationException)
                            { }
                            break;
                        case Config.SourceLocationEnum.HttpQueryString:
                            try
                            {
                                val = context.Request.Query[s.KeyName!].SingleOrDefault().TrimOrNull();
                                goto SetTheTenantId;
                            }
                            catch (InvalidOperationException)
                            { }
                            break;
                        default:
                            throw new UnexpectedSwitchValueException(s.SourceLocation);
                    }
                }
            }
            val ??= config.FallbackTenantId;
SetTheTenantId:
            TenantId = val;
            TenantIdFetched = true;
        }
        return Task.FromResult(TenantId);
    }
}

