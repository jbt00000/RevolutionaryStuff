using System.Security.Claims;

namespace RevolutionaryStuff.ApiCore.Services.PrincipalAccessors;

public class SoftClaimsPrincipalAccessor : IClaimsPrincipalAccessor
{
    public SoftClaimsPrincipalAccessor()
    { }

    public ClaimsPrincipal? ClaimsPrincipal { get; set; }
}
