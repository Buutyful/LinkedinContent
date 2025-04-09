using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Domain.UserDomain;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.Application.Authentication;

// Takes the provider name, the unique key from the provider, and the validated claims principal
public record ProcessExternalLoginCommand(
    string LoginProvider,
    string ProviderKey,
    ClaimsPrincipal Principal) : IRequest<ErrorOr<AuthenticationResult>>;
public class ProcessExternalLoginCommandHandler(
    UserManager<User> userManager,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<ProcessExternalLoginCommand, ErrorOr<AuthenticationResult>>
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    public async Task<ErrorOr<AuthenticationResult>> Handle(
        ProcessExternalLoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check if user is already linked with this external login
        
        if (await _userManager.FindByLoginAsync(request.LoginProvider, request.ProviderKey) is User user)
        {
            // User already linked, generate token
            var _roles = await _userManager.GetRolesAsync(user);
            var _token = _jwtTokenGenerator.GenerateToken(user, [.. _roles]);
            return new AuthenticationResult(user.Id, user.Email!, _token);
        }

        // 2. External login not found, check if email exists to link account
        var email = request.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            return Error.Validation("ExternalLogin.NoEmail", "Email claim not received from external provider.");
        }

       
        IdentityResult identityResult;
        var loginInfo = new UserLoginInfo(request.LoginProvider, request.ProviderKey, request.LoginProvider);
        var appUser = await _userManager.FindByEmailAsync(email);
        if (appUser is not null)
        {
            identityResult = await _userManager.AddLoginAsync(appUser, loginInfo);
        }
        else // No user with this email, create a new one
        {
            appUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = request.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                Email = email,
                EmailConfirmed = request
                .Principal
                .HasClaim(
                    c => c.Type == "email_verified" &&
                    c.Value.Equals("true", StringComparison.OrdinalIgnoreCase)),
                UserType = UserType.User
            };
            identityResult = await _userManager.CreateAsync(appUser);

            if (identityResult.Succeeded)
            {
                // Link the external login to the NEW user
                identityResult = await _userManager.AddLoginAsync(appUser, loginInfo);
            }
        }

        if (!identityResult.Succeeded)
        {
            return identityResult.Errors
                .Select(e => Error.Failure(e.Code, e.Description))
                .ToList();
        }

        // 3. User is now guaranteed to exist and be linked.
        var roles = await _userManager.GetRolesAsync(appUser!);
        var token = _jwtTokenGenerator.GenerateToken(appUser, [.. roles]);
        return new AuthenticationResult(appUser.Id, appUser.Email!, token);
    }
}