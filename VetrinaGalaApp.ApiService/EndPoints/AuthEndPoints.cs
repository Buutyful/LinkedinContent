﻿using MediatR;
using Microsoft.AspNetCore.Components.Authorization;
using VetrinaGalaApp.ApiService.Application.Authentication;

namespace VetrinaGalaApp.ApiService.EndPoints;

public static class AuthEndPoints
{
    public static IEndpointRouteBuilder MapAuthEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");
        {
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
        return app;
    }
}

public record LoginWithGoogleRequest(string IdToken);
public record AuthenticationRequest(string Email, string Password);
public record RegisterAsUserRequest(string UserName, string Email, string Password);
public record AuthenticationResult(Guid SubId, string Email, string Token);
public record ClaimDto(string Type, string Value);


