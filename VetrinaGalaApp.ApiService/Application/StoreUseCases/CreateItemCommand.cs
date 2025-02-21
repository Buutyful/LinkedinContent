using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Application.MiddlewareBehaviors;
using VetrinaGalaApp.ApiService.Domain.UserDomain;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.Application.StoreUseCases;

[Authorize(Policy = PolicyCostants.StoreOwner)]
public record CreateItemCommand(Guid ResourceOriginId, CreateItemRequest Item) : IAuthorizeableRequest<ErrorOr<Item>>;
public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(c => c.Item.Name).NotNull().NotEmpty().MaximumLength(100);
        RuleFor(c => c.Item.Description).NotNull().NotEmpty().MaximumLength(500);
        RuleFor(c => c.Item.Price).GreaterThan(0);
    }
}

public class CreateItemCommandHandler(AppDbContext context) : IRequestHandler<CreateItemCommand, ErrorOr<Item>>
{
    private readonly AppDbContext _appDbContext = context;

    public async Task<ErrorOr<Item>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        // Generate unique item ID
        var itemId = Guid.NewGuid();

        // Define MinIO object key and future image URL
        // TODO: Move these to configuration in a real application
        var minioServer = "http://minio-server:9000";
        var bucketName = "images";
        var objectKey = $"items/{itemId}/image.jpg";
        var imgUrl = $"{minioServer}/{bucketName}/{objectKey}";      

        // Create the item with all required fields
        var item = new Item
        {
            Id = itemId,
            Name = request.Item.Name,
            Description = request.Item.Description,
            Price = request.Item.Price,
            Currency = Currency.Euro, // TODO: Allow client to specify currency if needed
            ImgUrl = imgUrl,
            StoreId = request.ResourceOriginId,
            MMR = RatingMetrics.DefaultInitialMmr,
            LikeCount = 0,
            DislikeCount = 0
        };

        // Save the item to the database
        _appDbContext.Items.Add(item);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        return item;
    }
}
