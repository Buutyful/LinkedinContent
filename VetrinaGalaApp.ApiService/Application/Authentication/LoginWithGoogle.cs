using ErrorOr;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.Application.Authentication;
public record LoginWithGoogleCommand(string IdToken) : IRequest<ErrorOr<AuthenticationResult>>;

public class LoginWithGoogleCommandHandler(
    UserManager<User> userManager,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginWithGoogleCommand, ErrorOr<AuthenticationResult>>
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    public async Task<ErrorOr<AuthenticationResult>> Handle(
        LoginWithGoogleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate the Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
            var provider = "Google";
            var providerKey = payload.Subject; // Google's unique user ID

            // Find user by external login
            var user = await _userManager.FindByLoginAsync(provider, providerKey);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtTokenGenerator.GenerateToken(user, [.. roles]);
                return new AuthenticationResult(user.Id, user.Email!, token);
            }
            else
            {               
                var email = payload.Email;
                var userName = email.Split("@").First();

                // Check if email is already in use
                if (await _userManager.FindByEmailAsync(email) != null)
                {
                    return Error.Conflict(description: "Email already in use by another user.");
                }

                user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = userName,
                    Email = email
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return createResult.Errors
                        .Select(e => Error.Validation(e.Code, e.Description))
                        .ToList();
                }

                // Add external login
                var loginInfo = new UserLoginInfo(provider, providerKey, provider);
                var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
                if (!addLoginResult.Succeeded)
                {
                    return addLoginResult.Errors
                        .Select(e => Error.Validation(e.Code, e.Description))
                        .ToList();
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtTokenGenerator.GenerateToken(user, [.. roles]);
                return new AuthenticationResult(user.Id, user.Email, token);
            }
        }
        catch
        {
            return Error.Unauthorized(description: "Invalid Google ID token.");
        }
    }
}