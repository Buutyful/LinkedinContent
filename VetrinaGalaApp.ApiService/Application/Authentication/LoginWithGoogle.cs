using ErrorOr;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.Application.Authentication;
public record LoginWithGoogleCommand(string IdToken) : IRequest<ErrorOr<AuthenticationResult>>;

public class LoginWithGoogleQueryHandler : IRequestHandler<LoginWithGoogleCommand, ErrorOr<AuthenticationResult>>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;

    public LoginWithGoogleQueryHandler(
        UserManager<User> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
    }

    public async Task<ErrorOr<AuthenticationResult>> Handle(
        LoginWithGoogleCommand request,
        CancellationToken cancellationToken)
    {
        
        var clientId = _configuration["Google:ClientId"];
        if (string.IsNullOrEmpty(clientId))
        {
            throw new InvalidOperationException("Google Client ID is not configured.");
        }

        // Set up validation settings with audience check
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [clientId] // Ensure the token is for this app
        };

        try
        {
            // Validate the Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);
            var provider = "Google";
            var providerKey = payload.Subject; // Google's unique user ID

            // Check if the user exists by external login
            var user = await _userManager.FindByLoginAsync(provider, providerKey);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtTokenGenerator.GenerateToken(user, roles.ToList());
                return new AuthenticationResult(user.Id, user.Email!, token);
            }

            
            var email = payload.Email;
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                return Error.Conflict(description: "Email is already in use by another account.");
            }

            user = new User
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return createResult.Errors
                    .Select(e => Error.Validation(e.Code, e.Description))
                    .ToList();
            }

            // Link Google login to the user
            var loginInfo = new UserLoginInfo(provider, providerKey, provider);
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded)
            {
                return addLoginResult.Errors
                    .Select(e => Error.Validation(e.Code, e.Description))
                    .ToList();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var newUserToken = _jwtTokenGenerator.GenerateToken(user, userRoles.ToList());
            return new AuthenticationResult(user.Id, user.Email, newUserToken);
        }
        catch
        {
            return Error.Unauthorized(description: $"Invalid Google ID token: {ex.Message}");
        }
    }
}