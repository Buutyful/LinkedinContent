using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VetrinaGalaApp.ApiService.Domain;

namespace VetrinaGalaApp.ApiService.Infrastructure.Security;
public interface IJwtTokenGenerator
{
    //TODO: add relevant claims
    public string GenerateToken(User user);
}
public class JwtTokenGenerator(IOptions<JwtSettings> jwtOptions) : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;
    public string GenerateToken(User user)
    {

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //TODO: add relevant claims
        var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email!),
            };

        if (user.UserType == UserType.StoreOwner && user.Store is not null)
        {
            claims.Add(new Claim("OwnedStoreId", user.Store.Id.ToString()));
        }
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class JtwClaimTypesConstants
{
    public static string OwnedStoreId => "OwnedStoreId";
}