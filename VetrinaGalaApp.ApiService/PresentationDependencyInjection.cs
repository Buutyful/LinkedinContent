using Microsoft.AspNetCore.Authorization;
using VetrinaGalaApp.ApiService.EndPoints;

namespace VetrinaGalaApp.ApiService;

public static class PresentationDependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddHttpContextAccessor();
        services.AddAuthorizationBuilder()
            .AddPolicy("SellerOrAdmin", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin", "Seller");
            });


        services.AddSingleton<IAuthorizationHandler, StoreOwnerAuthorizationHandler>();
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
}
