using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Application.MiddlewareBehaviors;
using VetrinaGalaApp.ApiService.Domain;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure;

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
        //TODO: create proper item factory methods
        var item = new Item()
        {
            Id = Guid.NewGuid(),
            Name = request.Item.Name,
        };

        _appDbContext.Items.Add(item);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        return item;
    }
}
