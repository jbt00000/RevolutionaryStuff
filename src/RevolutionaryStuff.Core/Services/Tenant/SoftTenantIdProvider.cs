namespace RevolutionaryStuff.Core.Services.Tenant;

internal class SoftTenantIdProvider : ISoftTenantIdProvider
{
    public SoftTenantIdProvider()
    { }

    public string TenantId
    {
        get => field;
        set
        {
            if (field != null && field != value)
            {
                throw new NotNowException($"TenantId has already been set to {field}, cannot set it to {value}");
            }
            field = value;
        }
    }

    string ITenantIdProvider.GetTenantId()
        => TenantId;
}
