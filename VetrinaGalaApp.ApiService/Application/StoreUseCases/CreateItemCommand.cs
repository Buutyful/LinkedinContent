using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Application.MiddlewareBehaviors;
using VetrinaGalaApp.ApiService.Domain.UserDomain;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure;
using VetrinaGalaApp.ApiService.Infrastructure.MinIo;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.Application.StoreUseCases;

[Authorize(Policy = PolicyCostants.StoreOwner)]
public record CreateItemCommand(Guid ResourceOriginId, CreateItemRequest Item) : IAuthorizeableRequest<ErrorOr<(Item Item, string UploadUrl)>>;

public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(c => c.Item.Name).NotNull().NotEmpty().MaximumLength(100);
        RuleFor(c => c.Item.Description).NotNull().NotEmpty().MaximumLength(500);
        RuleFor(c => c.Item.Price).GreaterThan(0);
    }
}

public class CreateItemCommandHandler(
    AppDbContext context,
    IMinioService minioService) : IRequestHandler<CreateItemCommand, ErrorOr<(Item Item, string UploadUrl)>>
{
    private readonly AppDbContext _appDbContext = context;
    private readonly IMinioService _minioService = minioService;

    public async Task<ErrorOr<(Item Item, string UploadUrl)>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        
        var itemId = Guid.NewGuid();

        // Define the object key for MinIO storage
        var objectKey = $"items/{itemId}/image.jpg";

        // Get the public URL for the future image
        var imgUrl = await _minioService.GetPublicObjectUrl(objectKey);

        // Generate presigned URL for upload
        var presignedUrl = await _minioService.GeneratePresignedPutUrl(objectKey);

        
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

        _appDbContext.Items.Add(item);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        return (item, presignedUrl);
    }
}