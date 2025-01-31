using Microsoft.AspNetCore.Authorization;
using VetrinaGalaApp.ApiService.Infrastructure.Security.Jwt;

public interface IStoreResource
{
    Guid StoreId { get; }
}
public class StoreOwnerRequirement : IAuthorizationRequirement
{
}
public sealed record JustStoreId(Guid StoreId) : IStoreResource;
public class StoreOwnerAuthorizationHandler : AuthorizationHandler<StoreOwnerRequirement, IStoreResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StoreOwnerRequirement requirement,
        IStoreResource resource)
    {
        var storeId = context.User.Claims.Single(claim => claim.Type == JtwClaimTypesConstants.OwnedStoreId)?.Value;

        if (Guid.TryParse(storeId, out var res) && res == resource.StoreId)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}