using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using VetrinaGalaApp.ApiService.Application.Authentication;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Domain.UserDomain;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.EndPoints;

public static class AuthEndPoints
{
    public static IEndpointRouteBuilder MapAuthEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");
        {
            group.MapGet("/login/google-initiate", (
               HttpContext httpContext,
               SignInManager<User> signInManager,
               string? returnUrl = "/") => // Optional: Where to redirect within SPA after full login          
            {
                var provider = "Google";
                
                var callbackUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/signin-google";

                // Configure properties for the external authentication challenge
                // The RedirectUri tells the middleware where Google should redirect back to.                
                var properties = signInManager.ConfigureExternalAuthenticationProperties(
                    provider,
                    callbackUrl);

                // Trigger the authentication challenge.
                // The Google middleware will intercept this and generate a 302 Redirect
                // response to Google's authentication endpoint.
                return Results.Challenge(properties, [provider]);
            });

            // This endpoint is used to login with Google when the client has already the id token (implicit flow)
            group.MapPost("/login/google", async (
            LoginWithGoogleRequest request,
            ISender sender) =>
            {
                var result = await sender.Send(new LoginWithGoogleCommand(request.IdToken));
                return result.Match(
                    authResult => Results.Ok(authResult),
                    errors => errors.ToResult());
            });

            group.MapPost("/register", async (
            RegisterAsUserRequest request,
            ISender sender) =>
            {
                var registerCommand = new RegisterCommand(
                    request.UserName,
                    request.Email,
                    request.Password);

                var result = await sender.Send(registerCommand);

                return result.Match(
                    res => Results.Ok(res),
                    errors => errors.ToResult());
            });


            group.MapPost("/login", async (
              AuthenticationRequest request,
              ISender sender) =>
            {
                var loginQuery = new LoginQuery(request.Email, request.Password);

                var result = await sender.Send(loginQuery);

                return result.Match(
                    res => Results.Ok(res),
                    errors => errors.ToResult());
            });

            group.MapGet("/check", () => Results.Ok())
                .RequireAuthorization();
        }
        app.MapGet("/auth/claims", (HttpContext context) =>
        {
            return context.User.Claims
                .Select(c => new ClaimDto(c.Type, c.Value))
                .ToList();
        }).RequireAuthorization();


        app.MapGet("/signin-google", async (
            HttpContext httpContext, // Needed for SignOutAsync if used
            SignInManager<User> signInManager,
            ISender sender) =>
        {
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Results.BadRequest();
            }

            // Optional: Clean up external cookie
            await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Pass the provider name, key, and principal obtained by the middleware
            var result = await sender.Send(
                new ProcessExternalLoginCommand(info.LoginProvider, info.ProviderKey, info.Principal));

            return result.Match(
                authResult => Results.Ok(authResult),
                errors => errors.ToResult());

        });
        return app;
    }
}

public record LoginWithGoogleRequest(string IdToken);
public record AuthenticationRequest(string Email, string Password);
public record RegisterAsUserRequest(string UserName, string Email, string Password);
public record AuthenticationResult(Guid SubId, string Email, string Token);
public record ClaimDto(string Type, string Value);


