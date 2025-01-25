using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VetrinaGalaApp.ApiService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    // Add service defaults & Aspire client integrations.
    builder.AddServiceDefaults();

    // Add services to the container.
    builder.Services.AddProblemDetails();
    builder.Services.AddInfrastructure(builder);
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("StoreOwnerOrAdmin", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin", "StoreOwner");
        });

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddSingleton<IAuthorizationHandler, StoreOwnerAuthorizationHandler>();
    builder.Services.AddOpenApi();
}


var app = builder.Build();
{
    // Configure the HTTP request pipeline.
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapDefaultEndpoints();

    app.MapGet("store/{storeId:guid}/items", async (Guid storeId, AppDbContext context, IAuthorizationService auth, HttpContext httpContext) =>
    {
        var authResult = await auth.AuthorizeAsync(httpContext.User, new JustStoreId(storeId), new StoreOwnerRequirement());
        if (!authResult.Succeeded)
        {
            return Results.Forbid();
        }
        var items = await context.Items.Where(i => i.StoreId == storeId).ToListAsync();
        return Results.Ok(items);
    }).RequireAuthorization("StoreOwnerOrAdmin");

    app.Run();
}

public interface IStoreResource
{
    Guid StoreId { get; }
}
public sealed record JustStoreId(Guid StoreId) : IStoreResource;
public class StoreOwnerRequirement : IAuthorizationRequirement
{
}
public class StoreOwnerAuthorizationHandler : AuthorizationHandler<StoreOwnerRequirement, IStoreResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StoreOwnerRequirement requirement,
        IStoreResource resource)
    {
        var storeId = context.User.Claims.Single(claim => claim.Type == "OwnedStoreId")?.Value;

        if (Guid.TryParse(storeId, out var res) && res == resource.StoreId)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}