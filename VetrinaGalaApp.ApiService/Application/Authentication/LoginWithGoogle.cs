﻿using ErrorOr;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Domain.UserDomain;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.Application.Authentication;
public record LoginWithGoogleCommand(string IdToken) : IRequest<ErrorOr<AuthenticationResult>>;

public class LoginWithGoogleCommandHandler(
    UserManager<User> userManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IConfiguration configuration) : IRequestHandler<LoginWithGoogleCommand, ErrorOr<AuthenticationResult>>
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly IConfiguration _configuration = configuration;

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
            Audience = [clientId]
        };


        // Validate the Google ID token
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);
        var provider = "Google";
        var providerKey = payload.Subject; // Google's unique user ID

        // Check if the user exists by external login
        if(await _userManager.FindByLoginAsync(provider, providerKey) is User user)       
        {
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, [.. roles]);
            return new AuthenticationResult(user.Id, user.Email!, token);
        }

        //Here u can decide to merge the google login with the existing email
        var email = payload.Email;
        if (await _userManager.FindByEmailAsync(email) != null)
        {
            return Error.Conflict(description: "Email is already in use by another account.");
        }

        user = new User
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            UserType = UserType.User
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
        var newUserToken = _jwtTokenGenerator.GenerateToken(user, [.. userRoles]);

        return new AuthenticationResult(user.Id, user.Email, newUserToken);

    }
}