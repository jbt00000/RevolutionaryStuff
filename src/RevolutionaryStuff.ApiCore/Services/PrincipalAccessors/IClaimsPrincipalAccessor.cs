using System.Security.Claims;

namespace RevolutionaryStuff.ApiCore.Services.PrincipalAccessors;
public interface IClaimsPrincipalAccessor
{
    ClaimsPrincipal? ClaimsPrincipal { get; }
}
