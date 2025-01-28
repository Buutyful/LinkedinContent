using MediatR;
using VetrinaGalaApp.ApiService.Application.Authentication;

namespace VetrinaGalaApp.ApiService.EndPoints;

public static class UserEndPoints
{
    public static IEndpointRouteBuilder MapUserEndPoints(this IEndpointRouteBuilder app)
    {        
        var group = app.MapGroup("/auth");

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

        return app;
    }
}

public record AuthenticationRequest(string Email, string Password);
public record RegisterAsUserRequest(string UserName, string Email, string Password);
public record AuthenticationResult(Guid SubId, string Email, string Token);


