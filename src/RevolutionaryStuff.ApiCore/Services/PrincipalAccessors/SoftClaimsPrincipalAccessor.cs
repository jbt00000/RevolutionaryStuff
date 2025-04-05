using System.Security.Claims;

namespace RevolutionaryStuff.ApiCore.Services.PrincipalAccessors;


internal class SoftClaimsPrincipalAccessor : ISoftClaimsPrincipalAccessor
{
    private ClaimsPrincipal? ClaimsPrincipal { get; set; }
    ClaimsPrincipal? ISoftClaimsPrincipalAccessor.ClaimsPrincipal { get => ClaimsPrincipal; set => ClaimsPrincipal = value; }

    ClaimsPrincipal? IClaimsPrincipalAccessor.ClaimsPrincipal => ClaimsPrincipal;
}
