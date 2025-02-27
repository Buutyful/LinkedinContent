using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Application.StoreUseCases;
using VetrinaGalaApp.ApiService.Domain.UserDomain;
using VetrinaGalaApp.ApiService.Infrastructure;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.EndPoints;

public static class StoreEndPoints
{
    public static IEndpointRouteBuilder MapStoreEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("store").RequireAuthorization(PolicyCostants.OwnsAStoreOrAdmin);
        {
            group.MapGet("/{storeId:guid}/items",
                async (Guid storeId, AppDbContext context,
                IAuthorizationService auth,
                HttpContext httpContext) =>
            {
                var authResult = await auth.AuthorizeAsync(httpContext.User, storeId, new StoreOwnerRequirement());
                if (!authResult.Succeeded)
                {
                    return Results.Forbid();
                }
                var items = await context.Items.Where(i => i.StoreId == storeId).ToListAsync();
                return Results.Ok(items);
            });

            group.MapPost("/{storeId:guid}", async (Guid storeId, CreateItemRequest request, ISender sender) =>
            {
                var resp = await sender.Send(new CreateItemCommand(storeId, request));
                return resp.Match(
                    result => Results.Ok(new
                    {
                        Item = (ItemDto)result.Item,
                        UploadUrl = result.UploadUrl
                    }),
                    errors => errors.ToResult());
            });

        }

        app.MapPost("store/register", async Task<IResult> (
              CreateStoreRequest request,
              ICurrentUserProvider userProvider,
              ISender sender) =>
            {
                var currentUser = userProvider.GetUser();

                if (currentUser.UserType is UserType.StoreOwner)
                {
                    return Results.BadRequest();
                }
                var result = await sender.Send(new CreateStoreCommand(currentUser.Id, request));

                return result.Match(
                    res => Results.Ok(res),
                    errors => errors.ToResult());

            }).RequireAuthorization();

        return app;
    }
}

public record ItemDto(Guid Id, string Name, string Description, decimal Price, string ImgUrl)
{
    public static implicit operator ItemDto(Item item) =>
        new(item.Id, item.Name, item.Description, item.Price, item.ImgUrl);
}
public record StoreDto(Guid Id, string Name, string Description, Guid UserId);
public record CreateItemRequest(
    string Name,
    string Description,
    decimal Price,
    string Catalog);

public record CreateStoreRequest(string Name, string Description);