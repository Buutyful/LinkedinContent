using Microsoft.AspNetCore.Identity;
using VetrinaGalaApp.ApiService.Domain;

namespace VetrinaGalaApp.ApiService.Application.Common.Security;

public record CurrentUser(Guid Id, string Email, UserType UserType, Guid? StoreId);
public interface ICurrentUserProvider
{
    public CurrentUser GetUser();
}
public interface IJwtTokenGenerator
{
    //TODO: add relevant claims
    public string GenerateToken(User user, List<string> roles);
}