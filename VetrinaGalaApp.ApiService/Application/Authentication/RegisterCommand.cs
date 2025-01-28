using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using VetrinaGalaApp.ApiService.Application.Validators;
using VetrinaGalaApp.ApiService.Domain;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure.Security;

namespace VetrinaGalaApp.ApiService.Application.Authentication;

public record RegisterCommand(
    string UserName,
    string Email,
    string Password) : IRequest<ErrorOr<AuthenticationResult>>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.UserName).UserNameValidator();
        RuleFor(x => x.Password).PasswordValidator();
    }
}

public class RegisterCommandHandler(
    IJwtTokenGenerator jwtTokenGenerator,
    UserManager<User> userManager) : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    public async Task<ErrorOr<AuthenticationResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        //check if user already exists
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Error.Conflict(description: "email already in use by another user");
        }
        if (await _userManager.FindByNameAsync(request.UserName) is not null)
        {
            return Error.Conflict(description: "username already in use by another user");
        }

        //create user
        var user = new User { Id = Guid.NewGuid(), UserName = request.UserName, Email = request.Email };

        //add user
        var res = await _userManager.CreateAsync(user, request.Password);

        if (!res.Succeeded) return res.Errors.Select(e =>
        Error.Unexpected(code: e.Code, description: e.Description)).ToList();

        //generate token

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthenticationResult(user.Id, user.Email, token);
    }
}