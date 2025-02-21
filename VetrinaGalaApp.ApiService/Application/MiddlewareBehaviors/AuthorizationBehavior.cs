using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace VetrinaGalaApp.ApiService.Application.MiddlewareBehaviors;

public interface IAuthorizeableRequest<T> : IRequest<T>
{
    public Guid ResourceOriginId { get; }
}

public class AuthorizationBehavior<TRequest, TResponse>
    (IAuthorizationService authorizationService,
     IHttpContextAccessor httpContextAccessor) :
    IPipelineBehavior<TRequest, TResponse>
     where TRequest : IAuthorizeableRequest<TResponse>
     where TResponse : IErrorOr
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext ??
        throw new ArgumentNullException();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var authorizeAttributes = typeof(TRequest)
            .GetCustomAttributes<AuthorizeAttribute>(true)
            .ToList();

        if (authorizeAttributes.Count == 0)
        {
            return await next();
        }

        // Retrive Policies metadata from attribute
        var policies = authorizeAttributes
            .Select(a => a.Policy)
            .Where(policy => !string.IsNullOrEmpty(policy))
            .ToList();

        if (policies.Count == 0)
        {
            throw new InvalidOperationException(
                $"Request {typeof(TRequest).Name} must specify at least one policy.");
        }

        // Create authorization tasks for all policies
        var authTasks = policies.Select(policy =>
            _authorizationService.AuthorizeAsync(
                _httpContext.User,
                request.ResourceOriginId,
                policy!)).ToList();

        var authResults = await Task.WhenAll(authTasks);

        // All policies must pass, if an or behavior is needed change to Any
        if (!authResults.All(res => res.Succeeded))
        {
            var failedPolicies = policies
                 .Zip(authResults, (p, r) => new { Policy = p, r.Succeeded })
                 .Where(x => !x.Succeeded)
                 .Select(x => x.Policy);

            var error = Error.Forbidden($"Authorization failed for policies: {string.Join(", ", failedPolicies)}");

            var errorOr = ErrorOr<TResponse>.From([error]);
            return (TResponse)(IErrorOr)errorOr;
        }

       return await next();
    }
}
