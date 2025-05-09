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
            if (!field)
            { 
                field = !EqualityComparer<TTenantId>.Default.Equals(TenantId, default);
            }
            return field;
        }
        private set;
    }

    public TTenantId TenantId 
    {
        get
        {
            if (!HasTenantId && !EqualityComparer<TTenantId>.Default.Equals(ConfigOptions.Value.TenantId, default))
            {
                field = ConfigOptions.Value.TenantId;
                HasTenantId = true;
            }
            return field;
        }
        set
        {
            if (HasTenantId && !EqualityComparer<TTenantId>.Default.Equals(field, default) && !EqualityComparer<TTenantId>.Default.Equals(field, value))
            {
                throw new NotNowException($"TenantId has already been set to {field}, cannot set it to {value}");
            }
            field = value;
            HasTenantId = true;
        }
    }

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
