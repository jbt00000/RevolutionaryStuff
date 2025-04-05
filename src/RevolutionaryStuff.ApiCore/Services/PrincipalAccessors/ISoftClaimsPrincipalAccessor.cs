using System.Security.Claims;

namespace RevolutionaryStuff.ApiCore.Services.PrincipalAccessors;

public interface ISoftClaimsPrincipalAccessor : IClaimsPrincipalAccessor
{
    new ClaimsPrincipal? ClaimsPrincipal { get; set; }
}
