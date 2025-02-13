using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Domain.UserDomain;
using VetrinaGalaApp.ApiService.Infrastructure.Security.Jwt;

namespace VetrinaGalaApp.ApiService.Infrastructure.Security;

public class UserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    private readonly HttpContext _context = httpContextAccessor.HttpContext
        ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public CurrentUser GetUser()
    {
        var claimsPrincipal = _context.User;

        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Missing sub claim");

        var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email)
            ?? throw new InvalidOperationException("Missing email claim");

        var storeId = claimsPrincipal.FindFirstValue(JtwClaimTypesConstants.OwnedStoreId);

        return new CurrentUser(
            Id: Guid.Parse(userId),
            Email: email,
            UserType: !string.IsNullOrEmpty(storeId) ? UserType.StoreOwner : UserType.User,
            StoreId: string.IsNullOrEmpty(storeId) ? null : Guid.Parse(storeId));
    }
}