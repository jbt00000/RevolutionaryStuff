using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Core.ApplicationParts;

public class TenantProvider<TTenantId> : ITenantProvider<TTenantId>
    where TTenantId : IEquatable<TTenantId>
{
    public class Config
    {
        public const string ConfigSectionName = "TenantProvider";
        public TTenantId TenantId { get; set; }
    }

    private readonly IOptions<Config> ConfigOptions;

    public bool HasTenantId
    {
        get
        {
            if (HasTenantIdField == null)
            {
                var tid = ConfigOptions.Value.TenantId;
                HasTenantIdField = !EqualityComparer<TTenantId>.Default.Equals(tid, default);
                if (HasTenantIdField == true)
                {
                    TenantIdField = tid;
                }
            }
            return HasTenantIdField.Value;
        }
    }
    private bool? HasTenantIdField;

    public TTenantId TenantId
    {
        get
        {
            _ = HasTenantId;
            return TenantIdField;
        }
        set
        {
            if (HasTenantIdField == true)
            {
                if (TenantIdField.Equals(value))
                {
                    return;
                }
                else
                {
                    throw new NotNowException($"TenantId has already been set to {TenantIdField}, cannot set it to {value}");
                }
            }
            TenantIdField = value;
            HasTenantIdField = true;
        }
    }
    private TTenantId TenantIdField;

    Task<TTenantId> ITenantFinder<TTenantId>.GetTenantIdAsync()
        => Task.FromResult(TenantId);

    public TenantProvider(IOptions<Config> configOptions)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        ConfigOptions = configOptions;
    }

    public TenantProvider(TTenantId tenantId)
        : this(new OptionsWrapper<Config>(new() { TenantId = tenantId }))
    { }
}
