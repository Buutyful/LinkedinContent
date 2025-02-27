using ErrorOr;
using MediatR;
using VetrinaGalaApp.ApiService.Infrastructure;

namespace VetrinaGalaApp.ApiService.Application.StoreUseCases.Images;

public record UpdateItemImageUrlCommand(Guid ItemId, string ImageUrl) : IRequest<ErrorOr<Success>>;

public class UpdateItemImageUrlCommandHandler(AppDbContext context) :
    IRequestHandler<UpdateItemImageUrlCommand, ErrorOr<Success>>
{
    private readonly AppDbContext _context = context;

    public async Task<ErrorOr<Success>> Handle(UpdateItemImageUrlCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Items.FindAsync([request.ItemId], cancellationToken: cancellationToken);
        if (item == null)
            return Error.NotFound("Item not found");

        item.ImgUrl = request.ImageUrl;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}

