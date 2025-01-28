using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace VetrinaGalaApp.ApiService.Application.MiddlewareBehaviors;

public interface IAuthorizeableRequest<T> : IRequest<T>
{
    public Guid ResourceOriginId { get; }
}
//TODO: for now, there's only one policy and one authhandler,
//to extend this, mark the request with an attribute and extract here in the behavior its meta data(polices, roles)
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
        var authResult = await _authorizationService.AuthorizeAsync(
            _httpContext.User,
            new JustStoreId(request.ResourceOriginId),
            new StoreOwnerRequirement());

        if (!authResult.Succeeded)
        {            
            var error = Error.Forbidden("You don't have permission to access this resource");

            var errorOr = ErrorOr<TResponse>.From([error]);

            return (TResponse)(IErrorOr)errorOr;
        }

       return await next();
    }
}
