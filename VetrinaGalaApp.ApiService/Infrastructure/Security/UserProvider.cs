using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Domain;

namespace VetrinaGalaApp.ApiService.Infrastructure.Security;

public class UserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    private readonly HttpContext _context = httpContextAccessor.HttpContext ??
        throw new ArgumentNullException();
    public CurrentUser GetUser()
    {
        var claimsPrincipal = _context.User;

        var userId = Guid.Parse(claimsPrincipal.Claims
            .Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);

        var email = claimsPrincipal.Claims
            .Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value;

        var (userType, storeId) = DetermineUserTypeAndStore(claimsPrincipal);

        return new CurrentUser(userId, email, userType, storeId);
    }

    private static (UserType UserType, Guid? StoreId) DetermineUserTypeAndStore(ClaimsPrincipal user)
    {
        var storeIdClaim = user.Claims
            .FirstOrDefault(claim => claim.Type == "OwnedStoreId")?.Value;

        if (string.IsNullOrEmpty(storeIdClaim))
        {
            return (UserType.User, null);
        }

        return Guid.TryParse(storeIdClaim, out var storeId)
            ? (UserType.StoreOwner, storeId)
            : (UserType.User, null);
    }
}