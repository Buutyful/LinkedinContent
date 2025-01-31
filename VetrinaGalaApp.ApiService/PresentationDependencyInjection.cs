using Microsoft.AspNetCore.Authorization;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.EndPoints;

namespace VetrinaGalaApp.ApiService;

public static class PresentationDependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddHttpContextAccessor();
        services.AddAuthorizationRequirments();
        
        services.AddOpenApi();

        return services;
    }

    public static IEndpointRouteBuilder MapEndPoints(this IEndpointRouteBuilder app)
    {
        app
            .MapUserEndPoints()
            .MapStoreEndPoints();

        return app;
    }

    private static IServiceCollection AddAuthorizationRequirments(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
           .AddPolicy(PolicyCostants.OwnsAStoreOrAdmin, policy =>
           {
               policy.RequireAuthenticatedUser();
               policy.RequireRole(RoleConstants.Admin, RoleConstants.StoreOwner);
           })
           .AddPolicy(PolicyCostants.StoreOwner, policy => policy.AddRequirements(new StoreOwnerRequirement()));


        services.AddSingleton<IAuthorizationHandler, StoreOwnerAuthorizationHandler>();

        return services;
    }
}
