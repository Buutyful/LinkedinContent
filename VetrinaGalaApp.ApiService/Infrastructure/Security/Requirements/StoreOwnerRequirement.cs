using Microsoft.AspNetCore.Authorization;
using VetrinaGalaApp.ApiService.Infrastructure.Security.Jwt;


public class StoreOwnerRequirement : IAuthorizationRequirement
{
}
public class StoreOwnerAuthorizationHandler : AuthorizationHandler<StoreOwnerRequirement, Guid>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StoreOwnerRequirement requirement,
        Guid resource)
    {
        var storeId = context.User.Claims.Single(claim => claim.Type == JtwClaimTypesConstants.OwnedStoreId)?.Value;

        if (Guid.TryParse(storeId, out var res) && res == resource)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}