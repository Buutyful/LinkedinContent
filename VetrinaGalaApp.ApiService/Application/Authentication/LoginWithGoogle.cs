using ErrorOr;
using Google.Apis.Auth;
using MediatR;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure.Security;

namespace VetrinaGalaApp.ApiService.Application.Authentication;


public record LoginWithGoogleCommand(string IdToken) : IRequest<ErrorOr<AuthenticationResult>>;

//If a google login wasnt already present, creates a new user with the email from the token
public class LoginWithGoogleCommandHandler(
    IOptions<GoogleSettings> googleSettings,
    ISender sender) : IRequestHandler<LoginWithGoogleCommand, ErrorOr<AuthenticationResult>>
{
    private readonly GoogleSettings _googleSettings = googleSettings.Value;
    private readonly ISender _sender = sender;

    public async Task<ErrorOr<AuthenticationResult>> Handle(
        LoginWithGoogleCommand request,
        CancellationToken cancellationToken)
    {
        var clientId = _googleSettings.ClientId;
        if (string.IsNullOrEmpty(clientId))
        {
            throw new InvalidOperationException("Google Client ID is not configured.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);
        }
        catch (InvalidJwtException ex)
        {
            // Log ex for details
            return Error.Validation("Google.InvalidToken", "Invalid Google ID token.");
        }

        // Create a ClaimsPrincipal from the validated payload
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, payload.Subject),
            new Claim(ClaimTypes.Email, payload.Email),
            new Claim("email_verified", payload.EmailVerified.ToString(), ClaimValueTypes.Boolean),
            new Claim(ClaimTypes.Name, payload.Name)
        };
        var identity = new ClaimsIdentity(claims, "Google");
        var principal = new ClaimsPrincipal(identity);

        // Delegate the core user processing and JWT generation to the shared handler
        var provider = "Google";
        var providerKey = payload.Subject; // Google's unique user ID
        
        return await _sender.Send(
            new ProcessExternalLoginCommand(provider, providerKey, principal),
            cancellationToken);
    }
}