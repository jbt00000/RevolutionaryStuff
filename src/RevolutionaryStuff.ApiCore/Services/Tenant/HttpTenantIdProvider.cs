using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.Tenant;

namespace RevolutionaryStuff.ApiCore.Services.Tenant;

internal class HttpTenantIdProvider(IOptions<HttpTenantIdProvider.Config> ConfigOptions, IHttpContextAccessor ContextAccessor, RevolutionaryStuffService.RevolutionaryStuffServiceConstrutorArgs baseConstructorArgs)
    : RevolutionaryStuffService(baseConstructorArgs), IHttpTenantIdProvider, ITenantIdHolder
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

    string ITenantIdHolder.TenantId
    {
        get => ((ITenantIdProvider)this).GetTenantId();
        set
        {
            if (TenantIdFetched && TenantId != value && !string.IsNullOrEmpty(TenantId))
            {
                throw new NotNowException($"TenantId has already been set, cannot change it from {TenantId} to {value}.");
            }
            TenantIdFetched = true;
            TenantId = value;
        }
    }

    string? ITenantIdProvider.GetTenantId()
    {
        if (!TenantIdFetched)
        {
            var config = ConfigOptions.Value;
            var context = ContextAccessor.HttpContext;
            var svals = "";
            if (context != null)
            {
                foreach (var s in config.Sources.NullSafeEnumerable().Where(z => z.IsUsable))
                {
                    switch (s.SourceLocation)
                    {
                        case Config.SourceLocationEnum.HttpHeaders:
                            try
                            {
                                var sv = context.Request.Headers[s.KeyName!];
                                if (sv.Count > 0)
                                {
                                    svals = svals + "," + sv.Format(",");
                                }
                            }
                            catch (InvalidOperationException)
                            { }
                            break;
                        case Config.SourceLocationEnum.HttpQueryString:
                            try
                            {
                                var sv = context.Request.Query[s.KeyName!];
                                if (sv.Count > 0)
                                {
                                    svals = svals + "," + sv.Format(",");
                                }
                            }
                            catch (InvalidOperationException)
                            { }
                            break;
                        default:
                            throw new UnexpectedSwitchValueException(s.SourceLocation);
                    }
                }
            }
            var vals = CSV.ParseLine(svals).Select(z => StringHelpers.TrimOrNull(z)).WhereNotNull().Distinct().ToArray();
            TenantId = vals.Length == 1 ? vals[0] : config.FallbackTenantId;
            TenantIdFetched = true;
        }
        return TenantId;
    }
}

